using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Location;

/// <summary>
/// <see cref="ITurkishLocationService"/> implementasyonu.
/// Sabit veri olduğu için <see cref="IRepository{TEntity}.GetAllAsync"/>
/// callback'ine cache key vererek 24 saatlik cache aktif edilir.
/// </summary>
public class TurkishLocationService : ITurkishLocationService
{
    #region Fields

    protected readonly IRepository<TurkishProvince> _provinceRepository;
    protected readonly IRepository<TurkishDistrict> _districtRepository;
    protected readonly IRepository<TurkishNeighborhood> _neighborhoodRepository;
    protected readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public TurkishLocationService(
        IRepository<TurkishProvince> provinceRepository,
        IRepository<TurkishDistrict> districtRepository,
        IRepository<TurkishNeighborhood> neighborhoodRepository,
        IStaticCacheManager staticCacheManager)
    {
        _provinceRepository = provinceRepository;
        _districtRepository = districtRepository;
        _neighborhoodRepository = neighborhoodRepository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region İl

    /// <inheritdoc />
    public virtual async Task<IList<TurkishProvince>> GetAllProvincesAsync()
    {
        return await _provinceRepository.GetAllAsync(
            query => from p in query
                     where p.Active
                     orderby p.DisplayOrder, p.Name
                     select p,
            _ => TurkeyCoreDefaults.Cache.ProvincesAll);
    }

    /// <inheritdoc />
    public virtual async Task<TurkishProvince?> GetProvinceByIdAsync(int provinceId)
    {
        if (provinceId <= 0)
            return null;

        return await _provinceRepository.GetByIdAsync(provinceId, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<TurkishProvince?> GetProvinceByPlateCodeAsync(int plateCode)
    {
        if (plateCode is < 1 or > 81)
            return null;

        // Tüm il listesi zaten cache'de — orada filtreleyerek ek sorgudan kaçınırız
        var all = await GetAllProvincesAsync();
        return all.FirstOrDefault(p => p.PlateCode == plateCode);
    }

    #endregion

    #region İlçe

    /// <inheritdoc />
    public virtual async Task<IList<TurkishDistrict>> GetDistrictsByProvinceIdAsync(int provinceId)
    {
        if (provinceId <= 0)
            return new List<TurkishDistrict>();

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkeyCoreDefaults.Cache.DistrictsByProvince, provinceId);

        return await _districtRepository.GetAllAsync(
            query => from d in query
                     where d.ProvinceId == provinceId && d.Active
                     orderby d.DisplayOrder, d.Name
                     select d,
            _ => key);
    }

    /// <inheritdoc />
    public virtual async Task<TurkishDistrict?> GetDistrictByIdAsync(int districtId)
    {
        if (districtId <= 0)
            return null;

        return await _districtRepository.GetByIdAsync(districtId, cache => default);
    }

    #endregion

    #region Mahalle

    /// <inheritdoc />
    public virtual async Task<IList<TurkishNeighborhood>> GetNeighborhoodsByDistrictIdAsync(int districtId)
    {
        if (districtId <= 0)
            return new List<TurkishNeighborhood>();

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkeyCoreDefaults.Cache.NeighborhoodsByDistrict, districtId);

        return await _neighborhoodRepository.GetAllAsync(
            query => from n in query
                     where n.DistrictId == districtId && n.Active
                     orderby n.Name
                     select n,
            _ => key);
    }

    /// <inheritdoc />
    public virtual async Task<IList<TurkishNeighborhood>> GetNeighborhoodsByPostalCodeAsync(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return new List<TurkishNeighborhood>();

        var normalized = postalCode.Trim();

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkeyCoreDefaults.Cache.NeighborhoodsByPostalCode, normalized);

        return await _neighborhoodRepository.GetAllAsync(
            query => from n in query
                     where n.PostalCode == normalized && n.Active
                     orderby n.Name
                     select n,
            _ => key);
    }

    /// <inheritdoc />
    public virtual async Task<TurkishNeighborhood?> GetNeighborhoodByIdAsync(int neighborhoodId)
    {
        if (neighborhoodId <= 0)
            return null;

        return await _neighborhoodRepository.GetByIdAsync(neighborhoodId, cache => default);
    }

    #endregion
}
