using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// TCMB döviz kuru geçmişi — günlük TCMB feed'inden çekilen kurların log kaydı
/// </summary>
public class ExchangeRateLog : BaseEntity
{
    /// <summary>
    /// Kaynak para birimi ISO kodu (örn. USD, EUR)
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// TCMB döviz alış kuru (TRY karşılığı)
    /// </summary>
    public decimal ForexBuying { get; set; }

    /// <summary>
    /// TCMB döviz satış kuru (TRY karşılığı)
    /// </summary>
    public decimal ForexSelling { get; set; }

    /// <summary>
    /// TCMB efektif alış kuru
    /// </summary>
    public decimal? BanknoteBuying { get; set; }

    /// <summary>
    /// TCMB efektif satış kuru
    /// </summary>
    public decimal? BanknoteSelling { get; set; }

    /// <summary>
    /// TCMB feed'inde belirtilen kur tarihi
    /// </summary>
    public DateTime RateDateUtc { get; set; }

    /// <summary>
    /// Kayıt zamanı (sistem tarafından)
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
