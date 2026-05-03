using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// Türkiye il (vilayet) — 81 adet sabit kayıt
/// </summary>
public class TurkishProvince : BaseEntity
{
    /// <summary>
    /// İl adı (örn. "İstanbul")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Plaka kodu (1-81)
    /// </summary>
    public int PlateCode { get; set; }

    /// <summary>
    /// Görüntüleme sırası — alfabetik veya plaka sırasına göre
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Aktif mi (deaktif edilirse storefront'ta görünmez)
    /// </summary>
    public bool Active { get; set; } = true;
}
