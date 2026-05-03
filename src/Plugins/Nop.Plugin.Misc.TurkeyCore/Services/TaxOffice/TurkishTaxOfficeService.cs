using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.TaxOffice;

/// <summary>
/// <see cref="ITurkishTaxOfficeService"/> implementasyonu.
/// Liste seyrek değişir → 24 saat cache. CRUD sonrası key'ler invalide edilir.
/// </summary>
public class TurkishTaxOfficeService : ITurkishTaxOfficeService
{
    #region Fields

    protected readonly IRepository<TurkishTaxOffice> _taxOfficeRepository;
    protected readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public TurkishTaxOfficeService(
        IRepository<TurkishTaxOffice> taxOfficeRepository,
        IStaticCacheManager staticCacheManager)
    {
        _taxOfficeRepository = taxOfficeRepository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Read

    /// <inheritdoc />
    public virtual async Task<IList<TurkishTaxOffice>> GetAllAsync()
    {
        return await _taxOfficeRepository.GetAllAsync(
            query => from t in query
                     where t.Active
                     orderby t.ProvinceId, t.Name
                     select t,
            _ => TurkeyCoreDefaults.Cache.TaxOfficesAll);
    }

    /// <inheritdoc />
    public virtual async Task<IList<TurkishTaxOffice>> GetByProvinceIdAsync(int provinceId)
    {
        if (provinceId <= 0)
            return new List<TurkishTaxOffice>();

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkeyCoreDefaults.Cache.TaxOfficesByProvince, provinceId);

        return await _taxOfficeRepository.GetAllAsync(
            query => from t in query
                     where t.ProvinceId == provinceId && t.Active
                     orderby t.Name
                     select t,
            _ => key);
    }

    /// <inheritdoc />
    public virtual async Task<TurkishTaxOffice?> GetByIdAsync(int taxOfficeId)
    {
        if (taxOfficeId <= 0)
            return null;

        return await _taxOfficeRepository.GetByIdAsync(taxOfficeId, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<TurkishTaxOffice?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalized = code.Trim();
        var all = await GetAllAsync();
        return all.FirstOrDefault(t => t.Code == normalized);
    }

    #endregion

    #region Write

    /// <inheritdoc />
    public virtual async Task InsertAsync(TurkishTaxOffice taxOffice)
    {
        ArgumentNullException.ThrowIfNull(taxOffice);
        await _taxOfficeRepository.InsertAsync(taxOffice);
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(TurkishTaxOffice taxOffice)
    {
        ArgumentNullException.ThrowIfNull(taxOffice);
        await _taxOfficeRepository.UpdateAsync(taxOffice);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TurkishTaxOffice taxOffice)
    {
        ArgumentNullException.ThrowIfNull(taxOffice);
        await _taxOfficeRepository.DeleteAsync(taxOffice);
    }

    #endregion
}
