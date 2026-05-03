using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;

/// <summary>
/// Çerez onay (consent) kayıt servisi.
///
/// Anonim ziyaretçiler için <see cref="CookieConsent.ConsentGuid"/> tarayıcıdaki
/// HttpOnly cookie'de saklanır; sunucu tarafında her onay yeni satır olarak
/// audit amacıyla yazılır (KVKK ispat yükümlülüğü).
/// </summary>
public interface ICookieConsentService
{
    /// <summary>
    /// Yeni bir consent kaydı oluşturur. SHA-256 hash hesaplanır,
    /// IP/UA otomatik doldurulur. Çağıran controller'da kategori onaylarını set eder.
    /// </summary>
    Task<CookieConsent> RecordConsentAsync(
        Guid consentGuid,
        int customerId,
        bool functional,
        bool analytics,
        bool marketing,
        string? ipAddress,
        string? userAgent);

    /// <summary>
    /// Belirli bir consentGuid'in en güncel onay kaydını getirir.
    /// </summary>
    Task<CookieConsent?> GetLatestAsync(Guid consentGuid);

    /// <summary>
    /// Müşteri için en güncel onay kaydını getirir (login sonrası rıza birleştirme).
    /// </summary>
    Task<CookieConsent?> GetLatestForCustomerAsync(int customerId);
}
