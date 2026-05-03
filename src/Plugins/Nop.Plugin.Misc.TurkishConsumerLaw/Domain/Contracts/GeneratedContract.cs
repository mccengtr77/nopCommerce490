using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

/// <summary>
/// Sipariş anında üretilen sözleşme/form instance'ı.
///
/// MSS ve ÖBF aynı yapıda olduğu için tek tabloda Type ile ayrılır
/// (spec'te ayrı entity'ler önerilmişti, ancak DRY ve sorgu kolaylığı için birleştirildi).
///
/// Saklama: <see cref="TurkishConsumerLawDefaults.Retention.ContractRetentionYears"/> (default 3 yıl).
/// SHA-256 hash ile manipülasyon kontrolü.
/// </summary>
public class GeneratedContract : BaseEntity
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public ContractTemplateType Type { get; set; }

    /// <summary>Üretildiği şablon (versiyon takibi için)</summary>
    public int TemplateId { get; set; }

    /// <summary>Üretildiği şablonun versiyonu (snapshot)</summary>
    public string TemplateVersion { get; set; } = "1.0";

    /// <summary>Token replace edilmiş HTML (kullanıcı bu içeriği onayladı)</summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Müşteri checkout sırasında "okudum kabul ediyorum" işaretledi mi.
    /// MSS'de zorunlu, ÖBF'de "okudum" yeterli.
    /// </summary>
    public bool Accepted { get; set; }

    public DateTime? AcceptedOnUtc { get; set; }

    public string AcceptanceIp { get; set; } = string.Empty;

    public string AcceptanceUserAgent { get; set; } = string.Empty;

    /// <summary>HTML içeriğin SHA-256 hash'i — manipülasyon kontrolü</summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>Saklama süresinin sonu (CreatedOn + 3 yıl)</summary>
    public DateTime ExpiresOnUtc { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    /// <summary>Mağaza Id (multi-store)</summary>
    public int StoreId { get; set; }
}
