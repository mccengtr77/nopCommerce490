using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;

/// <inheritdoc />
public class ExplicitConsentService : IExplicitConsentService
{
    protected readonly IRepository<ExplicitConsentText> _repository;
    protected readonly IStaticCacheManager _staticCacheManager;

    public ExplicitConsentService(
        IRepository<ExplicitConsentText> repository,
        IStaticCacheManager staticCacheManager)
    {
        _repository = repository;
        _staticCacheManager = staticCacheManager;
    }

    /// <inheritdoc />
    public virtual async Task<IList<ExplicitConsentText>> GetActiveAsync(int storeId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkishConsumerLawDefaults.Cache.ActiveExplicitConsentTexts, storeId);

        var allActive = await _repository.GetAllAsync(
            query => query.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder),
            _ => key);

        // Mağazaya özel (LimitedToStoreId == storeId) + global (0) hepsi gösterilir;
        // mağazaya özel varsa global'i ezer (scope başına dedup).
        var byScope = allActive
            .GroupBy(t => t.Scope)
            .Select(g => g.OrderByDescending(t => t.LimitedToStoreId == storeId).First())
            .OrderBy(t => t.DisplayOrder)
            .ToList();

        return byScope;
    }

    /// <inheritdoc />
    public virtual async Task<IList<ExplicitConsentText>> GetActiveKvkkAsync(int storeId)
    {
        var all = await GetActiveAsync(storeId);
        return all.Where(t => IsKvkkScope(t.Scope)).ToList();
    }

    /// <inheritdoc />
    public virtual async Task<IList<ExplicitConsentText>> GetActiveEtkAsync(int storeId)
    {
        var all = await GetActiveAsync(storeId);
        return all.Where(t => IsEtkScope(t.Scope)).ToList();
    }

    /// <inheritdoc />
    public virtual async Task<ExplicitConsentText?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<ExplicitConsentText?> GetByScopeAsync(ConsentScope scope, int storeId)
    {
        var all = await GetActiveAsync(storeId);
        return all.FirstOrDefault(t => t.Scope == scope);
    }

    /// <inheritdoc />
    public virtual async Task InsertAsync(ExplicitConsentText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        text.CreatedOnUtc = DateTime.UtcNow;
        await _repository.InsertAsync(text);
        await InvalidateCacheAsync();
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(ExplicitConsentText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        text.UpdatedOnUtc = DateTime.UtcNow;
        await _repository.UpdateAsync(text);
        await InvalidateCacheAsync();
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(ExplicitConsentText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        await _repository.DeleteAsync(text);
        await InvalidateCacheAsync();
    }

    /// <summary>KVKK kapsamlı scope'lar (1-9 aralığı).</summary>
    public static bool IsKvkkScope(ConsentScope scope) => (int)scope < 10;

    /// <summary>ETK ticari ileti scope'ları (10+).</summary>
    public static bool IsEtkScope(ConsentScope scope) => (int)scope >= 10;

    protected virtual async Task InvalidateCacheAsync()
    {
        await _staticCacheManager.RemoveByPrefixAsync(
            "Nop.Plugin.TurkishConsumerLaw.Kvkk.ExplicitConsents");
    }
}
