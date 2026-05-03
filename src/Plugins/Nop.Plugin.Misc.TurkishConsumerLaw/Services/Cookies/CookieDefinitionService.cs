using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;

/// <inheritdoc />
public class CookieDefinitionService : ICookieDefinitionService
{
    #region Fields

    protected readonly IRepository<CookieDefinition> _repository;
    protected readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public CookieDefinitionService(
        IRepository<CookieDefinition> repository,
        IStaticCacheManager staticCacheManager)
    {
        _repository = repository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual async Task<IList<CookieDefinition>> GetActiveAsync()
    {
        return await _repository.GetAllAsync(
            query => from c in query
                     where c.IsActive
                     orderby c.DisplayOrder, c.Name
                     select c,
            _ => TurkishConsumerLawDefaults.Cache.ActiveCookieDefinitions);
    }

    /// <inheritdoc />
    public virtual async Task<IList<CookieDefinition>> GetByCategoryAsync(CookieCategory category)
    {
        // Tüm aktifleri cache'den al, kategori filtresi memory-side
        var all = await GetActiveAsync();
        return all.Where(c => c.Category == category).ToList();
    }

    /// <inheritdoc />
    public virtual async Task<CookieDefinition?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task InsertAsync(CookieDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        await _repository.InsertAsync(definition);
        await InvalidateCacheAsync();
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(CookieDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        await _repository.UpdateAsync(definition);
        await InvalidateCacheAsync();
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(CookieDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        await _repository.DeleteAsync(definition);
        await InvalidateCacheAsync();
    }

    #endregion

    #region Helpers

    protected virtual async Task InvalidateCacheAsync()
    {
        await _staticCacheManager.RemoveAsync(TurkishConsumerLawDefaults.Cache.ActiveCookieDefinitions);
    }

    #endregion
}
