using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// Sepete eklenen ürünün fiyat snapshot'ı.
///
/// Ürünün <see cref="TurkishProductExtension"/> kaydı varsa, sepete eklenme anında
/// TCMB güncel kuruyla TRY karşılığı hesaplanır ve burada lock'lanır. Sepet hesaplaması
/// (<see cref="Nop.Core.Domain.Orders.GetShoppingCartItemUnitPriceEvent"/> hook üzerinden)
/// bu lock'lı fiyatı kullanır; aradan TCMB kuru değişse de sepetteki fiyat sabit kalır.
///
/// Sepetten kalem silinince <see cref="Nop.Core.Events.EntityDeletedEvent{ShoppingCartItem}"/>
/// consumer ile bu kayıt da temizlenir. Sipariş tamamlandığında ShoppingCartItem silindiği için
/// otomatik temizlenir; OrderItem zaten kendi UnitPrice snapshot'ını saklar.
/// </summary>
public class TurkishCartItemPriceLock : BaseEntity
{
    /// <summary>
    /// nopCommerce <see cref="Nop.Core.Domain.Orders.ShoppingCartItem"/> Id (FK)
    /// </summary>
    public int ShoppingCartItemId { get; set; }

    /// <summary>
    /// Lock anındaki TRY birim fiyat (primary store currency'de varsayılır).
    /// </summary>
    public decimal LockedUnitPrice { get; set; }

    /// <summary>
    /// Lock anında uygulanan TCMB satış kuru: 1 birim base currency = X TRY.
    /// Audit/troubleshoot için saklanır.
    /// </summary>
    public decimal LockedRate { get; set; }

    /// <summary>
    /// Lock anındaki base currency ISO kodu (USD/EUR vb.) — info amaçlı.
    /// </summary>
    public string BaseCurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Lock zamanı (UTC).
    /// </summary>
    public DateTime LockedAtUtc { get; set; }
}
