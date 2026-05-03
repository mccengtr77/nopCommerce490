using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// Türkiye vergi dairesi — GİB tarafından yayınlanan ~1000 adet kayıt.
/// E-fatura ve fatura kesim sürecinde kurumsal müşterilerden istenir.
/// </summary>
public class TurkishTaxOffice : BaseEntity
{
    /// <summary>
    /// Bağlı olduğu il Id
    /// </summary>
    public int ProvinceId { get; set; }

    /// <summary>
    /// Vergi dairesi adı (örn. "Beyoğlu Vergi Dairesi Başkanlığı")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// GİB vergi dairesi kodu (4 haneli)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Aktif mi (kapanan/birleşen daireler için)
    /// </summary>
    public bool Active { get; set; } = true;
}
