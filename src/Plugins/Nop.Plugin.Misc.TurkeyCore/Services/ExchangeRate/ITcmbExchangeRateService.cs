using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;

/// <summary>
/// TCMB (Türkiye Cumhuriyet Merkez Bankası) günlük döviz kuru servisi.
/// Kaynak: <see cref="TurkeyCoreDefaults.Tcmb.ExchangeRateFeedUrl"/>
/// TCMB feed'i hafta içi her iş günü 15:30 civarı güncellenir;
/// hafta sonu/tatil günlerinde son iş gününün kurları döner.
/// </summary>
public interface ITcmbExchangeRateService
{
    /// <summary>
    /// Para biriminin TRY karşılığındaki forex satış kurunu getirir (cache'li).
    /// TRY istenirse 1 döner.
    /// </summary>
    /// <param name="currencyCode">ISO kodu — USD, EUR, GBP, TRY vb.</param>
    /// <returns>Kur veya bulunamadıysa null.</returns>
    Task<decimal?> GetRateAsync(string currencyCode);

    /// <summary>
    /// İki para birimi arasında çapraz kur — TRY üzerinden hesaplanır.
    /// Örn. USD→EUR: USD/TRY ÷ EUR/TRY.
    /// </summary>
    Task<decimal?> GetCrossRateAsync(string sourceCurrency, string targetCurrency);

    /// <summary>
    /// TCMB feed'inden güncel kurları çeker, <see cref="ExchangeRateLog"/> tablosuna
    /// kaydeder ve cache'i invalide eder. Background task tarafından çağrılır.
    /// </summary>
    /// <returns>Kaydedilen kur sayısı.</returns>
    Task<int> RefreshRatesAsync();

    /// <summary>
    /// Belirli bir para biriminin tarihsel kur loglarını verir (admin için).
    /// </summary>
    Task<IList<ExchangeRateLog>> GetHistoricalRatesAsync(string currencyCode, DateTime fromUtc, DateTime toUtc);
}
