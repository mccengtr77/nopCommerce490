using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

/// <summary>
/// Müşteri (veya anonim ziyaretçi) çerez onay kaydı.
///
/// Yasal denetim için saklanır:
/// - KVKK m.5: Açık rıza ispatı veri sorumlusunda
/// - SHA-256 hash + IP/UA + timestamp ile manipülasyon kontrolü
/// - Versiyonlama: çerez politikası değişince yeni rıza istenir
///
/// Anonim ziyaretçiler için <see cref="ConsentGuid"/> tarayıcıya cookie olarak yazılır;
/// müşteri sonradan login olursa <see cref="CustomerId"/> ile ilişkilendirilebilir.
/// </summary>
public class CookieConsent : BaseEntity
{
    /// <summary>
    /// Kayıtlı müşteri Id (anonim ise 0)
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Anonim oturum tanımlayıcısı — tarayıcıya HttpOnly cookie olarak yazılır.
    /// Müşteri login olduğunda <see cref="CustomerId"/> ile birleştirilebilir.
    /// </summary>
    public Guid ConsentGuid { get; set; }

    /// <summary>
    /// İşlevsellik çerezleri için onay
    /// </summary>
    public bool FunctionalAllowed { get; set; }

    /// <summary>
    /// Analitik çerezler için onay
    /// </summary>
    public bool AnalyticsAllowed { get; set; }

    /// <summary>
    /// Pazarlama çerezleri için onay
    /// </summary>
    public bool MarketingAllowed { get; set; }

    /// <summary>
    /// Çerez politikası versiyonu — admin politikayı değiştirince
    /// versiyonu artırır; eski rıza geçersiz sayılır.
    /// </summary>
    public string PolicyVersion { get; set; } = "1.0";

    /// <summary>
    /// Onay anındaki IP adresi
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Onay anındaki tarayıcı UA bilgisi
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Tüm onay kayıt alanlarının SHA-256 hash'i — denetim için manipülasyon kontrolü
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    public DateTime AcceptedOnUtc { get; set; }
}
