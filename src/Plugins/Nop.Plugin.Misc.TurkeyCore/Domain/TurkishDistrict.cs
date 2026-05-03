using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// Türkiye ilçe — bir ile bağlı (~970 adet)
/// </summary>
public class TurkishDistrict : BaseEntity
{
    /// <summary>
    /// Bağlı olduğu il Id
    /// </summary>
    public int ProvinceId { get; set; }

    /// <summary>
    /// İlçe adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Görüntüleme sırası
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Aktif mi
    /// </summary>
    public bool Active { get; set; } = true;
}
