using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.TurkeyCore.Areas.Admin.Models;

/// <summary>
/// Admin product edit page'inde TurkeyCore widget'ı tarafından render edilen
/// "Türkiye - Döviz Bazlı Fiyat" panelinin view model'i.
/// </summary>
public record TurkishProductExtensionAdminModel : BaseNopModel
{
    /// <summary>nopCommerce Product Id (panel başlığında ve POST'ta kullanılır)</summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Seçili base currency Id (nopCommerce Currency tablosu).
    /// Null/0 = pasif (storefront'ta çevrim yok, Product.Price raw kullanılır).
    /// </summary>
    public int? BaseCurrencyId { get; set; }

    /// <summary>Base currency'deki birim fiyat</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Base currency'deki indirimsiz/karşılaştırma fiyatı (opsiyonel)</summary>
    public decimal? BaseOldPrice { get; set; }

    /// <summary>Base currency'deki ürün maliyeti (opsiyonel)</summary>
    public decimal? BaseProductCost { get; set; }

    /// <summary>Currency dropdown öğeleri (TRY de listede; admin TRY seçerse çevrim no-op)</summary>
    public IList<SelectListItem> AvailableCurrencies { get; set; } = new List<SelectListItem>();

    /// <summary>Şu anki TCMB satış kuru bilgisi (admin görsün diye, "100 USD = X TL")</summary>
    public string? RateInfo { get; set; }
}
