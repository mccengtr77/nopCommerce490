using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

/// <summary>
/// Sitede kullanılan çerezlerin kataloğu. Admin panelinden yönetilir,
/// storefront banner'ında "Çerez Detayları" linki ile listelenir.
///
/// KVKK Aydınlatma Yükümlülüğü Tebliği gereği veri sorumlusu, çerezleri
/// (amaç, süre, üçüncü taraf vb.) detaylı şekilde açıklamak zorundadır.
/// </summary>
public class CookieDefinition : BaseEntity
{
    /// <summary>
    /// Çerez adı (teknik) — örn. "_ga", "ASP.NET_SessionId"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Çerez sağlayıcısı — örn. "Google Analytics", "Site içi"
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Kullanım amacı (kullanıcıya gösterilen açıklama)
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Saklama süresi (insan okur metin) — örn. "2 yıl", "Oturum sonu"
    /// </summary>
    public string Duration { get; set; } = string.Empty;

    /// <summary>
    /// Çerez kategorisi (banner toggle'larıyla eşleşir)
    /// </summary>
    public CookieCategory Category { get; set; }

    /// <summary>
    /// Üçüncü taraf çerezi mi (yurtdışı veri aktarımı için önemli)
    /// </summary>
    public bool IsThirdParty { get; set; }

    /// <summary>
    /// Aktif mi (deaktif ise listede gösterilmez)
    /// </summary>
    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }
}
