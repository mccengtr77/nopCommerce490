using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.TurkeyCore.Areas.Admin.Models;
using Nop.Plugin.Misc.TurkeyCore.Services.Product;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.TurkeyCore.Areas.Admin.Components;

/// <summary>
/// Admin product edit/create page'inde <see cref="AdminWidgetZones.ProductDetailsBlock"/> zone'una
/// "Türkiye - Döviz Bazlı Fiyat" panelini render eder.
///
/// Plugin <see cref="TurkeyCorePlugin"/>'in <c>IWidgetPlugin.GetWidgetViewComponent</c> dönüşü
/// olarak bağlanır; widget zone caller'ı ProductModel'i <c>additionalData</c> olarak geçer.
///
/// Yeni ürün için (Id=0) de panel render edilir — admin form alanlarını doldurur,
/// standart "Kaydet" sonrası <c>EntityInsertedEvent</c> consumer'ı extension'ı insert eder.
///
/// "Pasif" seçeneği YOK: dropdown her zaman bir currency seçili gelir.
/// Default = mevcut extension'ın BaseCurrencyId'si veya yoksa site primary store currency.
/// Admin currency'yi store primary'de bırakırsa "döviz çevirmeyen extension" olur (TRY identity);
/// USD/EUR vb. seçerse TCMB kuru ile çevrim devreye girer.
/// </summary>
public class TurkishProductExtensionAdminViewComponent : NopViewComponent
{
    private readonly ITurkishProductExtensionService _extensionService;
    private readonly ICurrencyService _currencyService;
    private readonly IProductService _productService;
    private readonly CurrencySettings _currencySettings;

    public TurkishProductExtensionAdminViewComponent(
        ITurkishProductExtensionService extensionService,
        ICurrencyService currencyService,
        IProductService productService,
        CurrencySettings currencySettings)
    {
        _extensionService = extensionService;
        _currencyService = currencyService;
        _productService = productService;
        _currencySettings = currencySettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var productId = (additionalData as ProductModel)?.Id ?? 0;

        // Mevcut ürün için extension'ı yükle; yeni ürün için (Id=0) ext null kalır.
        var ext = productId > 0 ? await _extensionService.GetByProductIdAsync(productId) : null;
        var currencies = await _currencyService.GetAllCurrenciesAsync(showHidden: true);

        // Default selected currency: mevcut ext veya site primary store currency
        var selectedCurrencyId = ext?.BaseCurrencyId
            ?? _currencySettings.PrimaryStoreCurrencyId;

        var availableCurrencies = currencies.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Name} ({c.CurrencyCode})",
            Selected = selectedCurrencyId == c.Id
        }).ToList();

        // Default BasePrice: mevcut ext, yoksa Product.Price (mevcut ürün için), yoksa 0 (yeni ürün)
        decimal defaultBasePrice = ext?.BasePrice ?? 0m;
        if (ext is null && productId > 0)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product is not null)
                defaultBasePrice = product.Price;
        }

        string? rateInfo = null;
        if (ext is not null && ext.BaseCurrencyId is not null)
        {
            var converted = await _extensionService.ConvertBasePriceToTryAsync(ext);
            if (converted.HasValue)
                rateInfo = $"≈ {converted.Value:N2} ₺ (TCMB güncel kur)";
            else
                rateInfo = "Kur bulunamadı — TCMB feed'ini güncelleyin";
        }

        var model = new TurkishProductExtensionAdminModel
        {
            ProductId = productId,
            BaseCurrencyId = selectedCurrencyId,
            BasePrice = defaultBasePrice,
            BaseOldPrice = ext?.BaseOldPrice,
            BaseProductCost = ext?.BaseProductCost,
            AvailableCurrencies = availableCurrencies,
            RateInfo = rateInfo
        };

        return View("~/Plugins/Misc.TurkeyCore/Areas/Admin/Views/Shared/Components/TurkishProductExtensionAdmin/Default.cshtml", model);
    }
}
