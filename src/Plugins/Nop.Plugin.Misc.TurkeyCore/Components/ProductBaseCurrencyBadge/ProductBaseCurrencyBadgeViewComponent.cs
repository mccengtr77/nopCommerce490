using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkeyCore.Services.Product;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.TurkeyCore.Components.ProductBaseCurrencyBadge;

/// <summary>
/// Storefront'ta ürün fiyatının yanına "≈ 100.00 USD" gibi orijinal döviz fiyatını
/// küçük bir badge olarak basar.
///
/// Bağlandığı widget zone'lar:
/// - <see cref="Nop.Web.Framework.Infrastructure.PublicWidgetZones.ProductPriceBottom"/> — ürün detay sayfasında fiyatın altı
/// - <see cref="Nop.Web.Framework.Infrastructure.PublicWidgetZones.ProductBoxAddinfoMiddle"/> — kategori/listeleme product box'ı
///
/// Sadece üründe extension var ve <c>BaseCurrencyId</c> site primary store currency'sinden
/// farklıysa render eder. Aksi halde <c>Content(string.Empty)</c> — istemcide hiçbir şey görünmez.
/// </summary>
public class ProductBaseCurrencyBadgeViewComponent : NopViewComponent
{
    private readonly ITurkishProductExtensionService _extensionService;

    public ProductBaseCurrencyBadgeViewComponent(ITurkishProductExtensionService extensionService)
    {
        _extensionService = extensionService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var productId = additionalData switch
        {
            ProductDetailsModel pdm => pdm.Id,
            ProductOverviewModel pom => pom.Id,
            _ => 0
        };

        if (productId <= 0)
            return Content(string.Empty);

        var info = await _extensionService.GetForeignBasePriceAsync(productId);
        if (info is null)
            return Content(string.Empty);

        return View("~/Plugins/Misc.TurkeyCore/Components/ProductBaseCurrencyBadge/Default.cshtml", info.Value);
    }
}
