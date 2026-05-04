using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.CartLock;

/// <inheritdoc />
public class TurkishCartItemPriceLockService : ITurkishCartItemPriceLockService
{
    private readonly IRepository<TurkishCartItemPriceLock> _repo;

    public TurkishCartItemPriceLockService(IRepository<TurkishCartItemPriceLock> repo)
    {
        _repo = repo;
    }

    public virtual async Task<TurkishCartItemPriceLock?> GetByCartItemIdAsync(int shoppingCartItemId)
    {
        if (shoppingCartItemId <= 0)
            return null;

        var list = await _repo.GetAllAsync(
            query => query.Where(x => x.ShoppingCartItemId == shoppingCartItemId),
            getCacheKey: null);
        return list.FirstOrDefault();
    }

    public virtual async Task UpsertAsync(TurkishCartItemPriceLock @lock)
    {
        ArgumentNullException.ThrowIfNull(@lock);
        if (@lock.ShoppingCartItemId <= 0)
            throw new ArgumentException("ShoppingCartItemId belirlenmiş olmalı", nameof(@lock));

        if (@lock.Id == 0)
            await _repo.InsertAsync(@lock);
        else
            await _repo.UpdateAsync(@lock);
    }

    public virtual async Task DeleteByCartItemIdAsync(int shoppingCartItemId)
    {
        if (shoppingCartItemId <= 0)
            return;

        var existing = await _repo.GetAllAsync(
            query => query.Where(x => x.ShoppingCartItemId == shoppingCartItemId),
            getCacheKey: null);

        if (existing.Count > 0)
            await _repo.DeleteAsync(existing);
    }
}
