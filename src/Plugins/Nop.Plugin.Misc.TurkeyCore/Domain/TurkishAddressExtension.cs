using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// nopCommerce'in <see cref="Nop.Core.Domain.Common.Address"/> entity'sini
/// değiştirmeden Türkiye il/ilçe/mahalle ve bina/daire alanlarını paralel tutar.
/// </summary>
public class TurkishAddressExtension : BaseEntity
{
    /// <summary>
    /// nopCommerce Address Id (FK)
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// İl Id (TurkishProvince FK)
    /// </summary>
    public int? ProvinceId { get; set; }

    /// <summary>
    /// İlçe Id (TurkishDistrict FK)
    /// </summary>
    public int? DistrictId { get; set; }

    /// <summary>
    /// Mahalle Id (TurkishNeighborhood FK)
    /// </summary>
    public int? NeighborhoodId { get; set; }

    /// <summary>
    /// Adres etiketi/takma adı (kullanıcının verdiği — örn. "Ev", "Ofis", "Yazlık")
    /// </summary>
    public string? AdresAdi { get; set; }

    /// <summary>
    /// Bina/dış kapı no
    /// </summary>
    public string? BinaNo { get; set; }

    /// <summary>
    /// Daire/iç kapı no
    /// </summary>
    public string? DaireNo { get; set; }

    /// <summary>
    /// Müşteri tipi (bireysel / kurumsal). Varsayılan: bireysel.
    /// </summary>
    public TurkishCustomerType MusteriTipi { get; set; } = TurkishCustomerType.Individual;

    /// <summary>
    /// TC Kimlik No — şifreli saklanır (bireysel adres faturalama için)
    /// </summary>
    public string? TcKimlikNo { get; set; }

    /// <summary>
    /// Vergi Kimlik No — şifreli saklanır (kurumsal adres faturalama için)
    /// </summary>
    public string? VergiNo { get; set; }

    /// <summary>
    /// Vergi dairesi Id (TurkishTaxOffice FK, kurumsal)
    /// </summary>
    public int? VergiDairesiId { get; set; }
}
