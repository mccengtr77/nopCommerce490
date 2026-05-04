using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.CartLock;
using Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;
using Nop.Plugin.Misc.TurkeyCore.Services.Product;
using Nop.Services.Directory;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Events;

/// <summary>
/// Sepet kalemi fiyat snapshot mantığını yöneten consumer:
///
/// - <see cref="EntityInsertedEvent{ShoppingCartItem}"/>: Ürünün <see cref="TurkishProductExtension"/>
///   kaydı varsa o anki TCMB satış kuruyla TRY fiyat hesaplanıp <see cref="TurkishCartItemPriceLock"/>
///   tablosuna yazılır.
///
/// - <see cref="EntityDeletedEvent{ShoppingCartItem}"/>: Sepetten kalem çıkarılınca lock da temizlenir.
///
/// - <see cref="GetShoppingCartItemUnitPriceEvent"/>: Sepet hesaplama sırasında lock varsa
///   nopCommerce'in normal akışını <c>StopProcessing</c> ile durdurup lock'lı fiyatı dön.
///   Bu sayede aradan TCMB kuru değişse bile sepetteki fiyat sabit kalır.
///
/// Discount handling: MVP'de lock fiyatı üzerinde discount uygulanmaz (eventMessage.AppliedDiscounts boş).
/// Üründe extension yoksa StopProcessing tetiklenmez → standart akış devam eder ve
/// <see cref="Pricing.TurkishPriceCalculationService"/> üzerinden canlı çevrim yapılır.
/// </summary>
public class CartItemPriceLockConsumer :
    IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
    IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
    IConsumer<GetShoppingCartItemUnitPriceEvent>
{
    private readonly ITurkishProductExtensionService _extensionService;
    private readonly ITurkishCartItemPriceLockService _lockService;
    private readonly ICurrencyService _currencyService;
    private readonly ITcmbExchangeRateService _tcmbService;

    public CartItemPriceLockConsumer(
        ITurkishProductExtensionService extensionService,
        ITurkishCartItemPriceLockService lockService,
        ICurrencyService currencyService,
        ITcmbExchangeRateService tcmbService)
    {
        _extensionService = extensionService;
        _lockService = lockService;
        _currencyService = currencyService;
        _tcmbService = tcmbService;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
    {
        var sci = eventMessage.Entity;
        if (sci is null || sci.Id <= 0)
            return;

        var ext = await _extensionService.GetByProductIdAsync(sci.ProductId);
        if (ext is null || ext.BaseCurrencyId is null)
            return;

        var currency = await _currencyService.GetCurrencyByIdAsync(ext.BaseCurrencyId.Value);
        if (currency is null)
            return;

        var code = currency.CurrencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
            return;

        // TRY ise rate=1, çevirme yok ama lock yine de yazılır (sepette tutarlılık için).
        var rate = code is "TRY" or "TL"
            ? 1m
            : await _tcmbService.GetRateAsync(code);

        if (rate is null)
            return;  // Kur yoksa lock yapma; PriceCalculationService canlı override de döner false → Product.Price raw kullanılır

        var lockedUnitPrice = decimal.Round(ext.BasePrice * rate.Value, 4);

        await _lockService.UpsertAsync(new TurkishCartItemPriceLock
        {
            ShoppingCartItemId = sci.Id,
            LockedUnitPrice = lockedUnitPrice,
            LockedRate = rate.Value,
            BaseCurrencyCode = code,
            LockedAtUtc = DateTime.UtcNow
        });
    }

    public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
    {
        var sci = eventMessage.Entity;
        if (sci is null || sci.Id <= 0)
            return;

        await _lockService.DeleteByCartItemIdAsync(sci.Id);
    }

    public async Task HandleEventAsync(GetShoppingCartItemUnitPriceEvent eventMessage)
    {
        var sci = eventMessage.ShoppingCartItem;
        if (sci is null || sci.Id <= 0)
            return;

        var @lock = await _lockService.GetByCartItemIdAsync(sci.Id);
        if (@lock is null)
            return;  // lock yok → standart akış (canlı çevrim) devam eder

        eventMessage.UnitPrice = @lock.LockedUnitPrice;
        eventMessage.DiscountAmount = decimal.Zero;
        eventMessage.AppliedDiscounts = new List<Discount>();
        eventMessage.StopProcessing = true;
    }
}
