using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Product;

/// <summary>
/// Ürün başına döviz bazlı fiyat tanımı yönetim servisi.
///
/// İki sorumluluk:
/// 1. <see cref="TurkishProductExtension"/> CRUD — admin product edit panelinden çağrılır.
/// 2. **Persisted recalc**: Extension varsa <see cref="Nop.Core.Domain.Catalog.Product"/>'un
///    Price/OldPrice/ProductCost alanlarını TRY'ye çevirip <c>DB'ye yazar</c>. Bu sayede
///    nopCommerce'in tüm fiyat akışı (search, filter, discount, marketplace push, tier) tek
///    doğru fiyatı görür.
///
/// Recalc tetiklenen yerler:
/// - Admin "Kaydet" basınca (anlık, tek ürün)
/// - <c>ExchangeRateBackgroundTask</c> TCMB feed'i çektikten sonra (toplu, tüm extension'lı ürünler)
/// </summary>
public interface ITurkishProductExtensionService
{
    /// <summary>
    /// Bir ürünün döviz extension kaydını getirir; yoksa <c>null</c>.
    /// </summary>
    Task<TurkishProductExtension?> GetByProductIdAsync(int productId);

    /// <summary>
    /// Extension'ı insert veya update eder.
    /// Persist çağrılmaz — bunun için <see cref="RecalculateAndPersistAsync"/> çağrılmalı.
    /// </summary>
    Task UpsertAsync(TurkishProductExtension extension);

    /// <summary>
    /// Bir ürünün extension kaydını siler.
    /// <c>Product.Price</c> dokunulmaz (raw değer kalır) — admin gerekirse manuel düzeltir.
    /// </summary>
    Task DeleteByProductIdAsync(int productId);

    /// <summary>
    /// Bu ürünün <see cref="TurkishProductExtension.BasePrice"/>'ını TCMB güncel kuruyla
    /// TRY'ye çevirip <c>Product.Price/OldPrice/ProductCost</c> alanlarına yazar ve DB'ye persist eder.
    /// </summary>
    /// <returns>
    /// <c>true</c> — recalc uygulandı.
    /// <c>false</c> — extension yok / BaseCurrencyId null / currency bulunamadı / TCMB kuru yok.
    /// </returns>
    Task<bool> RecalculateAndPersistAsync(int productId);

    /// <summary>
    /// Tüm extension kayıtları için <see cref="RecalculateAndPersistAsync"/> çağırır.
    /// TCMB feed'i güncellendikten sonra <c>ExchangeRateBackgroundTask</c> tarafından tetiklenir.
    /// </summary>
    /// <returns>Recalc edilen ürün sayısı (kur olmayan/atlanan ürünler hariç).</returns>
    Task<int> RecalculateAllAsync();

    /// <summary>
    /// <paramref name="extension"/>'ın TCMB güncel kuruyla TRY karşılığını hesaplar (sadece display için,
    /// persist etmez). Admin paneli "≈ X TL" gösterimi için.
    /// </summary>
    Task<decimal?> ConvertBasePriceToTryAsync(TurkishProductExtension extension);

    /// <summary>
    /// Storefront'ta ürün fiyatının yanında "($100.00 USD)" gibi orijinal döviz fiyatını
    /// gösterilebilir bir tuple olarak döner.
    /// </summary>
    /// <returns>
    /// (basePrice, currencyCode, currency) tuple — eğer üründe extension var ve
    /// <c>BaseCurrencyId</c> site primary store currency'den farklıysa.
    /// Aksi halde <c>null</c> (badge gösterilmez).
    /// </returns>
    Task<(decimal basePrice, string currencyCode, Nop.Core.Domain.Directory.Currency currency)?>
        GetForeignBasePriceAsync(int productId);
}
