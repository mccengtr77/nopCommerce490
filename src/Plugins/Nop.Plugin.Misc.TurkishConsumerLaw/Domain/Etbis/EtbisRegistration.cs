using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Etbis;

/// <summary>
/// ETBİS (Elektronik Ticaret Bilgi Sistemi) kayıt bilgileri.
/// 6563 sayılı E-Ticaret Kanunu ve ETBİS Yönetmeliği gereği e-ticaret siteleri,
/// eticaret.gov.tr'den aldıkları karekodu site footer'ında göstermek zorundadır.
///
/// Genelde her mağaza için TEK aktif kayıt olur (multi-store senaryosunda store'a göre seçim).
/// </summary>
public class EtbisRegistration : BaseEntity
{
    /// <summary>
    /// 16 haneli MERSİS numarası
    /// </summary>
    public string MersisNo { get; set; } = string.Empty;

    /// <summary>
    /// Ticari ünvan (MERSİS'te kayıtlı)
    /// </summary>
    public string TradeName { get; set; } = string.Empty;

    /// <summary>
    /// ETBİS sicil kayıt tarihi
    /// </summary>
    public DateTime RegistrationDate { get; set; }

    /// <summary>
    /// eticaret.gov.tr üzerinden ETBİS kayıt sorgu URL'i
    /// (örn. https://eticaret.gov.tr/dogrulama/?id=...)
    /// </summary>
    public string VerificationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Footer'da render edilecek karekod HTML kodu — eticaret.gov.tr admin panelinden kopyalanır.
    /// Genelde &lt;img src="data:image/svg+xml;base64,..."/&gt; veya inline &lt;svg&gt; olur.
    /// HTML olduğu için XSS açısından admin tarafından girilmesi şart (untrusted input değil).
    /// </summary>
    public string QrCodeHtml { get; set; } = string.Empty;

    /// <summary>
    /// Multi-store: 0 = tüm mağazalara, &gt;0 = belirli mağaza
    /// </summary>
    public int LimitedToStoreId { get; set; }

    /// <summary>
    /// Aktif mi (sadece aktif kaydın karekodu render edilir)
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}
