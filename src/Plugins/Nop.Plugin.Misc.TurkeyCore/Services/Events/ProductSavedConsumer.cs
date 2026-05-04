using System.Globalization;
using Microsoft.AspNetCore.Http;
using Nop.Core.Events;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.Product;
using Nop.Services.Events;
using NopProduct = Nop.Core.Domain.Catalog.Product;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Events;

/// <summary>
/// Admin product edit/insert form'una gömülü "Türkiye - Döviz Bazlı Fiyat" panelinin
/// alanlarını standart "Kaydet" akışıyla işler.
///
/// Form alanları <c>TurkishProductExtension.X</c> prefix'iyle gelir
/// (bkz. <c>Areas/Admin/Views/Shared/Components/TurkishProductExtensionAdmin/Default.cshtml</c>).
/// Admin "Kaydet" basınca:
/// - Form'dan <c>BaseCurrencyId/BasePrice/BaseOldPrice/BaseProductCost</c> alanlarını oku
/// - <see cref="ITurkishProductExtensionService.UpsertAsync"/> ile extension'ı upsert et
/// - <see cref="ITurkishProductExtensionService.RecalculateAndPersistAsync"/> ile <c>Product.Price</c>
///   alanlarını yeni kurla DB'ye yaz
///
/// Dropdown'da "pasif" seçeneği yok — admin her zaman bir currency seçer (default = site primary
/// store currency). Primary currency seçilirse rate identity (1.0) → çevirme yok, ama extension
/// yine de tutulur (admin sonra döviz değiştirmek isterse mevcut değer korunur).
///
/// <strong>Loop koruması</strong>: Recalc <c>_productService.UpdateProductAsync</c> çağırır,
/// bu da <see cref="EntityUpdatedEvent{NopProduct}"/> tekrar publish eder. Loop'u önlemek için
/// <see cref="HttpContext.Items"/>'a flag set edilir; tekrar gelen event'lerde flag varsa skip.
/// </summary>
public class ProductSavedConsumer :
    IConsumer<EntityInsertedEvent<NopProduct>>,
    IConsumer<EntityUpdatedEvent<NopProduct>>
{
    private const string ProcessingFlag = "TurkeyCore.ProductSavedConsumer.Processing";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITurkishProductExtensionService _extensionService;

    public ProductSavedConsumer(
        IHttpContextAccessor httpContextAccessor,
        ITurkishProductExtensionService extensionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _extensionService = extensionService;
    }

    public Task HandleEventAsync(EntityInsertedEvent<NopProduct> eventMessage)
        => SyncFromFormAsync(eventMessage.Entity);

    public Task HandleEventAsync(EntityUpdatedEvent<NopProduct> eventMessage)
        => SyncFromFormAsync(eventMessage.Entity);

    private async Task SyncFromFormAsync(NopProduct product)
    {
        if (product is null || product.Id <= 0)
            return;

        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is null)
            return;

        // Loop koruması: bizim recalc'tan tetiklenen event ise skip
        if (ctx.Items.ContainsKey(ProcessingFlag))
            return;

        if (!ctx.Request.HasFormContentType)
            return;

        var form = ctx.Request.Form;

        // Form'da bizim panel alanı yoksa (örn. başka bir admin sayfasından product save), skip
        if (!form.ContainsKey("TurkishProductExtension.BaseCurrencyId"))
            return;

        var currencyId = ParseInt(form["TurkishProductExtension.BaseCurrencyId"]);
        if (currencyId is null or <= 0)
            return; // Beklenmeyen durum (dropdown'da pasif yok); savunma amaçlı skip

        var basePrice = ParseDecimal(form["TurkishProductExtension.BasePrice"]) ?? 0m;
        var baseOldPrice = ParseDecimal(form["TurkishProductExtension.BaseOldPrice"]);
        var baseProductCost = ParseDecimal(form["TurkishProductExtension.BaseProductCost"]);

        var existing = await _extensionService.GetByProductIdAsync(product.Id);
        var ext = existing ?? new TurkishProductExtension { ProductId = product.Id };
        ext.BaseCurrencyId = currencyId.Value;
        ext.BasePrice = basePrice;
        ext.BaseOldPrice = baseOldPrice;
        ext.BaseProductCost = baseProductCost;

        await _extensionService.UpsertAsync(ext);

        // Recalc Product.Price'ı yazar → EntityUpdatedEvent tekrar tetiklenir → flag ile skip
        ctx.Items[ProcessingFlag] = true;
        try
        {
            await _extensionService.RecalculateAndPersistAsync(product.Id);
        }
        finally
        {
            ctx.Items.Remove(ProcessingFlag);
        }
    }

    private static int? ParseInt(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private static decimal? ParseDecimal(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var normalized = raw.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;
    }
}
