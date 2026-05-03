using Nop.Plugin.Misc.TurkeyCore.Domain;
using NopAddress = Nop.Core.Domain.Common.Address;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Address;

/// <summary>
/// nopCommerce <see cref="NopAddress"/>'ı ile <see cref="TurkishAddressExtension"/>
/// arasındaki köprü servisi. nopCommerce'in kendi Address.StateProvinceId/ZipPostalCode
/// alanları ile Türkiye'nin Province/District/Neighborhood/PostalCode hiyerarşisini
/// senkronize tutar.
/// </summary>
public interface ITurkishAddressService
{
    /// <summary>
    /// Adrese ait extension kaydını getirir. Yoksa null.
    /// </summary>
    Task<TurkishAddressExtension?> GetExtensionAsync(NopAddress address);

    /// <summary>
    /// AddressId ile extension kaydını getirir. Yoksa null.
    /// </summary>
    Task<TurkishAddressExtension?> GetByAddressIdAsync(int addressId);

    /// <summary>
    /// Adrese ait extension kaydını getirir; yoksa default değerlerle yeni bir tane oluşturup kaydeder.
    /// </summary>
    Task<TurkishAddressExtension> GetOrCreateExtensionAsync(NopAddress address);

    /// <summary>
    /// Lokasyon alanlarını (il/ilçe/mahalle + bina/daire) tek seferde set eder.
    /// </summary>
    Task SetLocationAsync(NopAddress address,
        int? provinceId, int? districtId, int? neighborhoodId,
        string? binaNo = null, string? daireNo = null);

    /// <summary>
    /// Faturalama bilgilerini (müşteri tipi + TCKN bireysel veya VKN+VergiDairesi kurumsal)
    /// set eder. TCKN ve VKN <see cref="Nop.Services.Security.IEncryptionService"/> ile şifrelenir.
    /// Müşteri tipine uymayan alanlar otomatik temizlenir (örn. kurumsal ise TCKN null'lanır).
    /// </summary>
    Task SetInvoiceInfoAsync(NopAddress address,
        Domain.TurkishCustomerType musteriTipi,
        string? tcKimlikNo = null,
        string? vergiNo = null,
        int? vergiDairesiId = null);

    /// <summary>
    /// Adres etiketi (kullanıcı tarafından verilen takma ad — "Ev", "Ofis" vs.) ayarlar.
    /// </summary>
    Task SetAdresAdiAsync(NopAddress address, string? adresAdi);

    /// <summary>
    /// Extension kaydını ekler veya günceller.
    /// </summary>
    Task UpsertExtensionAsync(TurkishAddressExtension extension);

    /// <summary>
    /// İl/ilçe/mahalle adlarını döndüren ek model — adres yazdırma/PDF için.
    /// </summary>
    Task<TurkishAddressDetailsModel?> GetDetailsAsync(NopAddress address);
}

/// <summary>
/// İl/ilçe/mahalle/posta kodu adlarını içeren projeksiyon — adres yazdırma için.
/// </summary>
public record TurkishAddressDetailsModel(
    string? ProvinceName,
    string? DistrictName,
    string? NeighborhoodName,
    string? PostalCode,
    string? BinaNo,
    string? DaireNo);
