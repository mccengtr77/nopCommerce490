namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

/// <summary>
/// Çerez kategorileri — KVKK ve AB GDPR standartlarına uygun 4 kategorili tasarım.
/// Storefront banner'ında her kategori için ayrı toggle olur.
///
/// 6698 KVKK m.5 ve m.6: Açık rıza gerektiren kategoriler (Functional/Analytics/Marketing)
/// için kullanıcı onayı şart; Necessary onaysız çalışabilir.
/// </summary>
public enum CookieCategory
{
    /// <summary>
    /// Zorunlu çerezler — site temel işlevi için şart (oturum, sepet, csrf token).
    /// Onay gerekmez, kullanıcı kapatamaz.
    /// </summary>
    Necessary = 1,

    /// <summary>
    /// İşlevsellik çerezleri — kullanıcı tercihleri (dil, para birimi, tema).
    /// Onay gerekir.
    /// </summary>
    Functional = 2,

    /// <summary>
    /// Analitik çerezler — Google Analytics, Hotjar gibi izleme.
    /// Onay gerekir; hassasiyet açısından IP anonimleştirme önerilir.
    /// </summary>
    Analytics = 3,

    /// <summary>
    /// Pazarlama çerezleri — Facebook Pixel, Google Ads, retargeting.
    /// Onay gerekir; KVKK ve ETK kapsamında en hassas kategori.
    /// </summary>
    Marketing = 4
}
