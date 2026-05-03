using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Etbis;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Etbis;

/// <inheritdoc />
public class EtbisService : IEtbisService
{
    #region Fields

    protected readonly IRepository<EtbisRegistration> _repository;
    protected readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public EtbisService(
        IRepository<EtbisRegistration> repository,
        IStaticCacheManager staticCacheManager)
    {
        _repository = repository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual async Task<EtbisRegistration?> GetActiveAsync(int storeId)
    {
        // Tüm aktif kayıtları cache'le, mağaza filtresi memory-side yapılır
        var allActive = await _repository.GetAllAsync(
            query => query.Where(e => e.IsActive),
            _ => TurkishConsumerLawDefaults.Cache.ActiveEtbis);

        // Önce mağazaya özel, yoksa global (LimitedToStoreId=0)
        return allActive.FirstOrDefault(e => e.LimitedToStoreId == storeId)
            ?? allActive.FirstOrDefault(e => e.LimitedToStoreId == 0);
    }

    /// <inheritdoc />
    public virtual async Task<EtbisRegistration?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<IList<EtbisRegistration>> GetAllAsync()
    {
        return await _repository.GetAllAsync(
            query => query.OrderByDescending(e => e.IsActive).ThenBy(e => e.TradeName),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task InsertAsync(EtbisRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        registration.CreatedOnUtc = DateTime.UtcNow;
        await _repository.InsertAsync(registration);
        await InvalidateCacheAsync();
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(EtbisRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        registration.UpdatedOnUtc = DateTime.UtcNow;
        await _repository.UpdateAsync(registration);
        await InvalidateCacheAsync();
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(EtbisRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        await _repository.DeleteAsync(registration);
        await InvalidateCacheAsync();
    }

    #endregion

    #region Helpers

    protected virtual async Task InvalidateCacheAsync()
    {
        await _staticCacheManager.RemoveAsync(TurkishConsumerLawDefaults.Cache.ActiveEtbis);
    }

    #endregion
}
