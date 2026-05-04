using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// nopCommerce'in <see cref="Nop.Core.Domain.Catalog.Product"/> entity'sini değiştirmeden,
/// ürünün döviz bazlı fiyat tanımını paralel tutan extension tablosu.
///
/// İş kuralı: Admin "Bu ürünün fiyatı X dövizinde Y birim" der. Storefront tarafında
/// <see cref="Nop.Core.Domain.Catalog.Product.Price"/> her render'da TCMB güncel kuru ile
/// canlı olarak hesaplanır (storefront-only consumer; admin context'te override yok).
///
/// Sepete eklenince anlık kur ayrı bir snapshot tablosunda lock'lanır.
/// </summary>
public class TurkishProductExtension : BaseEntity
{
    /// <summary>
    /// nopCommerce Product Id (FK)
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// nopCommerce <see cref="Nop.Core.Domain.Directory.Currency"/> Id — soft FK.
    /// Null ise extension pasif kabul edilir (çevrim devre dışı).
    /// </summary>
    public int? BaseCurrencyId { get; set; }

    /// <summary>
    /// Ürünün base currency'sindeki birim fiyatı. BaseCurrencyId null ise anlamsız.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Base currency'deki indirimsiz/karşılaştırma fiyat (opsiyonel).
    /// </summary>
    public decimal? BaseOldPrice { get; set; }

    /// <summary>
    /// Base currency'deki ürün maliyeti (opsiyonel — kar/zarar raporları için).
    /// </summary>
    public decimal? BaseProductCost { get; set; }
}
