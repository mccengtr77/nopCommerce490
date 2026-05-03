using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;

/// <inheritdoc />
public class DisclosureTextService : IDisclosureTextService
{
    protected readonly IRepository<DisclosureText> _repository;
    protected readonly IStaticCacheManager _staticCacheManager;

    public DisclosureTextService(
        IRepository<DisclosureText> repository,
        IStaticCacheManager staticCacheManager)
    {
        _repository = repository;
        _staticCacheManager = staticCacheManager;
    }

    /// <inheritdoc />
    public virtual async Task<DisclosureText?> GetActiveAsync(int storeId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkeyCoreLikeFallback(), storeId);

        var allActive = await _repository.GetAllAsync(
            query => query.Where(t => t.IsActive).OrderByDescending(t => t.Id),
            _ => key);

        // Önce mağazaya özel, yoksa global (LimitedToStoreId=0)
        return allActive.FirstOrDefault(t => t.LimitedToStoreId == storeId)
            ?? allActive.FirstOrDefault(t => t.LimitedToStoreId == 0);
    }

    /// <inheritdoc />
    public virtual async Task<DisclosureText?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<IList<DisclosureText>> GetAllAsync()
    {
        return await _repository.GetAllAsync(
            query => query.OrderByDescending(t => t.IsActive).ThenBy(t => t.Title),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task InsertAsync(DisclosureText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        text.CreatedOnUtc = DateTime.UtcNow;
        await _repository.InsertAsync(text);
        await InvalidateCacheAsync();
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(DisclosureText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        text.UpdatedOnUtc = DateTime.UtcNow;
        await _repository.UpdateAsync(text);
        await InvalidateCacheAsync();
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(DisclosureText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        await _repository.DeleteAsync(text);
        await InvalidateCacheAsync();
    }

    private static CacheKey TurkeyCoreLikeFallback() =>
        TurkishConsumerLawDefaults.Cache.ActiveDisclosureText;

    protected virtual async Task InvalidateCacheAsync()
    {
        // Tüm storeId varyantlarını temizle — prefix tabanlı invalidasyon
        await _staticCacheManager.RemoveByPrefixAsync(
            "Nop.Plugin.TurkishConsumerLaw.Kvkk.DisclosureText");
    }
}
