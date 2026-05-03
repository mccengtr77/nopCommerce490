using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Location;

/// <summary>
/// Türkiye il/ilçe/mahalle hiyerarşisi sorgu servisi.
/// Veriler PTT ve sistem seed'inden gelir, 24 saat cache'lenir.
/// CRUD işlemleri admin panelinden yapılır — burada read-only API.
/// </summary>
public interface ITurkishLocationService
{
    #region İl

    /// <summary>
    /// Tüm aktif illeri DisplayOrder + ad sırasıyla getirir (cache'li).
    /// </summary>
    Task<IList<TurkishProvince>> GetAllProvincesAsync();

    /// <summary>
    /// Id ile il getirir.
    /// </summary>
    Task<TurkishProvince?> GetProvinceByIdAsync(int provinceId);

    /// <summary>
    /// Plaka kodu ile il getirir (1-81).
    /// </summary>
    Task<TurkishProvince?> GetProvinceByPlateCodeAsync(int plateCode);

    #endregion

    #region İlçe

    /// <summary>
    /// Bir ile bağlı tüm aktif ilçeleri ad sırasıyla getirir (cache'li).
    /// </summary>
    Task<IList<TurkishDistrict>> GetDistrictsByProvinceIdAsync(int provinceId);

    /// <summary>
    /// Id ile ilçe getirir.
    /// </summary>
    Task<TurkishDistrict?> GetDistrictByIdAsync(int districtId);

    #endregion

    #region Mahalle

    /// <summary>
    /// Bir ilçeye bağlı tüm aktif mahalleleri getirir (cache'li).
    /// Büyük ilçelerde 1000+ kayıt dönebilir; UI tarafında pagination/search önerilir.
    /// </summary>
    Task<IList<TurkishNeighborhood>> GetNeighborhoodsByDistrictIdAsync(int districtId);

    /// <summary>
    /// Posta kodu ile mahalle araması (5 hane).
    /// Aynı posta kodu birden fazla mahalleye atanmış olabilir.
    /// </summary>
    Task<IList<TurkishNeighborhood>> GetNeighborhoodsByPostalCodeAsync(string postalCode);

    /// <summary>
    /// Id ile mahalle getirir.
    /// </summary>
    Task<TurkishNeighborhood?> GetNeighborhoodByIdAsync(int neighborhoodId);

    #endregion
}
