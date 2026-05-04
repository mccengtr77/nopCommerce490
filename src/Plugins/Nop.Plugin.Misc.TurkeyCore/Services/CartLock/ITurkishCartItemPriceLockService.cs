using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.CartLock;

/// <summary>
/// Sepet kalemi fiyat snapshot CRUD servisi.
/// Sepete eklenince <see cref="UpsertAsync"/>, sepet hesaplamada <see cref="GetByCartItemIdAsync"/>,
/// kalem silinince <see cref="DeleteByCartItemIdAsync"/>.
/// </summary>
public interface ITurkishCartItemPriceLockService
{
    Task<TurkishCartItemPriceLock?> GetByCartItemIdAsync(int shoppingCartItemId);
    Task UpsertAsync(TurkishCartItemPriceLock @lock);
    Task DeleteByCartItemIdAsync(int shoppingCartItemId);
}
