using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// Türkiye mahalle/köy — bir ilçeye bağlı (~50.000+ adet, PTT veri tabanından)
/// </summary>
public class TurkishNeighborhood : BaseEntity
{
    /// <summary>
    /// Bağlı olduğu ilçe Id
    /// </summary>
    public int DistrictId { get; set; }

    /// <summary>
    /// Mahalle/köy adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Posta kodu (5 hane)
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Aktif mi
    /// </summary>
    public bool Active { get; set; } = true;
}
