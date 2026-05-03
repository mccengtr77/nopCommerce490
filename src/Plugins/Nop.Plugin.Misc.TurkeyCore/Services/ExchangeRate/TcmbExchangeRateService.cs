using System.Globalization;
using System.Xml.Linq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;

/// <summary>
/// <see cref="ITcmbExchangeRateService"/> implementasyonu.
///
/// TCMB feed XML formatı (today.xml):
/// <code>
/// &lt;Tarih_Date Tarih="03.05.2026" Date="05/03/2026" Bulten_No="..."&gt;
///   &lt;Currency CrossOrder="0" Kod="USD" CurrencyCode="USD"&gt;
///     &lt;Unit&gt;1&lt;/Unit&gt;
///     &lt;Isim&gt;ABD DOLARI&lt;/Isim&gt;
///     &lt;ForexBuying&gt;32.0345&lt;/ForexBuying&gt;
///     &lt;ForexSelling&gt;32.0987&lt;/ForexSelling&gt;
///     &lt;BanknoteBuying&gt;32.0123&lt;/BanknoteBuying&gt;
///     &lt;BanknoteSelling&gt;32.1456&lt;/BanknoteSelling&gt;
///   &lt;/Currency&gt;
///   ...
/// &lt;/Tarih_Date&gt;
/// </code>
/// Bazı para birimlerinde ForexBuying/Selling boş gelebilir (sadece Banknote olanlar).
/// Bu durumda kayıt atlanır.
/// </summary>
public class TcmbExchangeRateService : ITcmbExchangeRateService
{
    #region Fields

    protected readonly HttpClient _httpClient;
    protected readonly IRepository<ExchangeRateLog> _exchangeRateRepository;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly ILogger _logger;

    #endregion

    #region Ctor

    public TcmbExchangeRateService(
        HttpClient httpClient,
        IRepository<ExchangeRateLog> exchangeRateRepository,
        IStaticCacheManager staticCacheManager,
        ILogger logger)
    {
        _httpClient = httpClient;
        _exchangeRateRepository = exchangeRateRepository;
        _staticCacheManager = staticCacheManager;
        _logger = logger;
    }

    #endregion

    #region Read

    /// <inheritdoc />
    public virtual async Task<decimal?> GetRateAsync(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return null;

        var code = currencyCode.Trim().ToUpperInvariant();
        if (code == "TRY")
            return 1m;

        var rates = await GetLatestRatesFromCacheAsync();
        return rates.TryGetValue(code, out var rate) ? rate : null;
    }

    /// <inheritdoc />
    public virtual async Task<decimal?> GetCrossRateAsync(string sourceCurrency, string targetCurrency)
    {
        var source = await GetRateAsync(sourceCurrency);
        var target = await GetRateAsync(targetCurrency);

        if (source is null or 0 || target is null or 0)
            return null;

        // Her iki kur TRY karşılığı; çapraz: source/target → kaynaktan hedefe çevirme oranı
        return source.Value / target.Value;
    }

    /// <inheritdoc />
    public virtual async Task<IList<ExchangeRateLog>> GetHistoricalRatesAsync(
        string currencyCode, DateTime fromUtc, DateTime toUtc)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return new List<ExchangeRateLog>();

        var code = currencyCode.Trim().ToUpperInvariant();

        // Tarihsel sorgu cache'lenmez — admin/raporlama amaçlıdır, az çağrılır
        return await _exchangeRateRepository.GetAllAsync(
            query => from r in query
                     where r.CurrencyCode == code
                           && r.RateDateUtc >= fromUtc
                           && r.RateDateUtc <= toUtc
                     orderby r.RateDateUtc descending
                     select r,
            getCacheKey: null);
    }

    #endregion

    #region Refresh

    /// <inheritdoc />
    public virtual async Task<int> RefreshRatesAsync()
    {
        string xml;
        try
        {
            xml = await _httpClient.GetStringAsync(TurkeyCoreDefaults.Tcmb.ExchangeRateFeedUrl);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"TCMB feed indirilemedi: {ex.Message}", ex);
            throw;
        }

        IList<ExchangeRateLog> records;
        try
        {
            records = ParseFeedXml(xml);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"TCMB XML parse hatası: {ex.Message}", ex);
            throw;
        }

        if (records.Count == 0)
            return 0;

        await _exchangeRateRepository.InsertAsync(records);

        // Cache invalide et — bir sonraki GetRateAsync çağrısında yeniden yüklenir
        await _staticCacheManager.RemoveAsync(TurkeyCoreDefaults.Cache.LatestExchangeRates);

        return records.Count;
    }

    #endregion

    #region XML Parser

    /// <summary>
    /// TCMB today.xml içeriğini <see cref="ExchangeRateLog"/> listesine çevirir.
    /// Test edilebilirlik için statik ve saf — IO yapmaz.
    /// </summary>
    /// <exception cref="InvalidOperationException">Tarih_Date kök elementi yoksa.</exception>
    public static IList<ExchangeRateLog> ParseFeedXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return new List<ExchangeRateLog>();

        var doc = XDocument.Parse(xml);
        var root = doc.Root
            ?? throw new InvalidOperationException("TCMB XML root elementi boş");

        if (root.Name.LocalName != "Tarih_Date")
            throw new InvalidOperationException(
                $"Beklenmeyen kök element: '{root.Name.LocalName}' (Tarih_Date bekleniyordu)");

        var rateDateUtc = ParseTcmbDate(root.Attribute("Tarih")?.Value);
        var createdOnUtc = DateTime.UtcNow;

        var records = new List<ExchangeRateLog>();
        foreach (var currency in root.Elements("Currency"))
        {
            var code = currency.Attribute("CurrencyCode")?.Value
                       ?? currency.Attribute("Kod")?.Value;
            if (string.IsNullOrWhiteSpace(code))
                continue;

            var unit = ParseDecimalOrNull(currency.Element("Unit")?.Value) ?? 1m;
            var forexBuying = ParseDecimalOrNull(currency.Element("ForexBuying")?.Value);
            var forexSelling = ParseDecimalOrNull(currency.Element("ForexSelling")?.Value);
            var banknoteBuying = ParseDecimalOrNull(currency.Element("BanknoteBuying")?.Value);
            var banknoteSelling = ParseDecimalOrNull(currency.Element("BanknoteSelling")?.Value);

            // Forex kuru olmayan para birimleri (Unit > 1 ile gelen bazı sembolik kayıtlar) atlanır
            if (forexBuying is null || forexSelling is null)
                continue;

            // Unit > 1 ise (örn. JPY 100 birim için) birim başına normalize et
            records.Add(new ExchangeRateLog
            {
                CurrencyCode = code.Trim().ToUpperInvariant(),
                ForexBuying = forexBuying.Value / unit,
                ForexSelling = forexSelling.Value / unit,
                BanknoteBuying = banknoteBuying / unit,
                BanknoteSelling = banknoteSelling / unit,
                RateDateUtc = rateDateUtc,
                CreatedOnUtc = createdOnUtc
            });
        }

        return records;
    }

    /// <summary>
    /// "dd.MM.yyyy" formatındaki TCMB tarihini UTC DateTime'a çevirir.
    /// Parse edilemezse <see cref="DateTime.UtcNow"/>'un tarihi kullanılır.
    /// </summary>
    private static DateTime ParseTcmbDate(string? tarih)
    {
        if (!string.IsNullOrWhiteSpace(tarih) &&
            DateTime.TryParseExact(tarih, "dd.MM.yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
        }

        return DateTime.UtcNow.Date;
    }

    /// <summary>
    /// TCMB XML'inde sayısal alanlar nokta-ondalık (Invariant) biçimindedir,
    /// boş string olabilir. Hata vermez, null döner.
    /// </summary>
    private static decimal? ParseDecimalOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// En son kur dictionary'sini cache'den getirir; cache miss durumunda
    /// <see cref="ExchangeRateLog"/> tablosundaki en güncel <see cref="ExchangeRateLog.RateDateUtc"/>
    /// tarihli kurları yükler.
    /// </summary>
    protected virtual async Task<IDictionary<string, decimal>> GetLatestRatesFromCacheAsync()
    {
        Func<Task<IDictionary<string, decimal>>> loader = async () =>
        {
            var all = await _exchangeRateRepository.GetAllAsync(
                query => query.OrderByDescending(r => r.RateDateUtc).ThenByDescending(r => r.CreatedOnUtc),
                getCacheKey: null);

            if (all.Count == 0)
                return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            // En güncel tarihteki kayıtları al, currency başına en son CreatedOn
            var latestDate = all[0].RateDateUtc;
            return all
                .Where(r => r.RateDateUtc == latestDate)
                .GroupBy(r => r.CurrencyCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.CreatedOnUtc).First().ForexSelling,
                    StringComparer.OrdinalIgnoreCase);
        };

        return await _staticCacheManager.GetAsync(
            TurkeyCoreDefaults.Cache.LatestExchangeRates, loader);
    }

    #endregion
}
