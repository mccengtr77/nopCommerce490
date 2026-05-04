using Nop.Core.Caching;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Logging;
using NopProduct = Nop.Core.Domain.Catalog.Product;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Product;

/// <inheritdoc />
public class TurkishProductExtensionService : ITurkishProductExtensionService
{
    #region Fields

    protected readonly IRepository<TurkishProductExtension> _extensionRepository;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly ICurrencyService _currencyService;
    protected readonly ITcmbExchangeRateService _tcmbService;
    protected readonly IProductService _productService;
    protected readonly ILogger _logger;
    protected readonly CurrencySettings _currencySettings;

    #endregion

    #region Ctor

    public TurkishProductExtensionService(
        IRepository<TurkishProductExtension> extensionRepository,
        IStaticCacheManager staticCacheManager,
        ICurrencyService currencyService,
        ITcmbExchangeRateService tcmbService,
        IProductService productService,
        ILogger logger,
        CurrencySettings currencySettings)
    {
        _extensionRepository = extensionRepository;
        _staticCacheManager = staticCacheManager;
        _currencyService = currencyService;
        _tcmbService = tcmbService;
        _productService = productService;
        _logger = logger;
        _currencySettings = currencySettings;
    }

    #endregion

    #region CRUD

    /// <inheritdoc />
    public virtual async Task<TurkishProductExtension?> GetByProductIdAsync(int productId)
    {
        if (productId <= 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkeyCoreDefaults.Cache.ProductExtensionByProductId, productId);

        return await _staticCacheManager.GetAsync(key, async () =>
        {
            var list = await _extensionRepository.GetAllAsync(
                query => query.Where(e => e.ProductId == productId),
                getCacheKey: null);
            return list.FirstOrDefault();
        });
    }

    /// <inheritdoc />
    public virtual async Task UpsertAsync(TurkishProductExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        if (extension.ProductId <= 0)
            throw new ArgumentException("ProductId belirlenmiş olmalı", nameof(extension));

        if (extension.Id == 0)
            await _extensionRepository.InsertAsync(extension);
        else
            await _extensionRepository.UpdateAsync(extension);

        await InvalidateCacheAsync(extension.ProductId);
    }

    /// <inheritdoc />
    public virtual async Task DeleteByProductIdAsync(int productId)
    {
        if (productId <= 0)
            return;

        var existing = await _extensionRepository.GetAllAsync(
            query => query.Where(e => e.ProductId == productId),
            getCacheKey: null);

        if (existing.Count > 0)
            await _extensionRepository.DeleteAsync(existing);

        await InvalidateCacheAsync(productId);
    }

    #endregion

    #region Persisted Recalc

    /// <inheritdoc />
    public virtual async Task<bool> RecalculateAndPersistAsync(int productId)
    {
        if (productId <= 0)
            return false;

        var extension = await GetByProductIdAsync(productId);
        if (extension is null || extension.BaseCurrencyId is null)
            return false;

        var rate = await GetEffectiveRateAsync(extension.BaseCurrencyId.Value);
        if (rate is null)
            return false;

        var product = await _productService.GetProductByIdAsync(productId);
        if (product is null)
            return false;

        product.Price = decimal.Round(extension.BasePrice * rate.Value, 4);

        if (extension.BaseOldPrice.HasValue)
            product.OldPrice = decimal.Round(extension.BaseOldPrice.Value * rate.Value, 4);

        if (extension.BaseProductCost.HasValue)
            product.ProductCost = decimal.Round(extension.BaseProductCost.Value * rate.Value, 4);

        await _productService.UpdateProductAsync(product);
        return true;
    }

    /// <inheritdoc />
    public virtual async Task<int> RecalculateAllAsync()
    {
        var allExtensions = await _extensionRepository.GetAllAsync(
            query => query.Where(e => e.BaseCurrencyId != null),
            getCacheKey: null);

        var success = 0;
        var skipped = 0;

        foreach (var ext in allExtensions)
        {
            try
            {
                if (await RecalculateAndPersistAsync(ext.ProductId))
                    success++;
                else
                    skipped++;
            }
            catch (Exception ex)
            {
                // Bir ürünün recalc'ı başarısız olursa diğerlerini bloklamayalım
                await _logger.WarningAsync(
                    $"TurkishProductExtension recalc başarısız (ProductId={ext.ProductId}): {ex.Message}", ex);
                skipped++;
            }
        }

        await _logger.InformationAsync(
            $"Döviz bazlı ürün fiyatları recalc edildi: {success} başarılı, {skipped} atlandı (kur yok / ürün silinmiş / hata)");

        return success;
    }

    #endregion

    #region Display helper

    /// <inheritdoc />
    public virtual async Task<decimal?> ConvertBasePriceToTryAsync(TurkishProductExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        if (extension.BaseCurrencyId is null)
            return null;

        var rate = await GetEffectiveRateAsync(extension.BaseCurrencyId.Value);
        if (rate is null)
            return null;

        return decimal.Round(extension.BasePrice * rate.Value, 4);
    }

    /// <inheritdoc />
    public virtual async Task<(decimal basePrice, string currencyCode, Currency currency)?>
        GetForeignBasePriceAsync(int productId)
    {
        if (productId <= 0)
            return null;

        var ext = await GetByProductIdAsync(productId);
        if (ext is null || ext.BaseCurrencyId is null)
            return null;

        // Aynı primary currency ise badge gösterme — kullanıcı zaten o fiyatı görüyor
        if (ext.BaseCurrencyId.Value == _currencySettings.PrimaryStoreCurrencyId)
            return null;

        var currency = await _currencyService.GetCurrencyByIdAsync(ext.BaseCurrencyId.Value);
        if (currency is null || string.IsNullOrEmpty(currency.CurrencyCode))
            return null;

        return (ext.BasePrice, currency.CurrencyCode, currency);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Verilen Currency için TCMB güncel forex satış kurunu döner.
    /// "TRY"/"TL" identity (1), bilinmeyen currency null.
    /// </summary>
    protected virtual async Task<decimal?> GetEffectiveRateAsync(int currencyId)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(currencyId);
        if (currency is null)
            return null;

        var code = currency.CurrencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
            return null;

        if (code is "TRY" or "TL")
            return 1m;

        return await _tcmbService.GetRateAsync(code);
    }

    protected virtual async Task InvalidateCacheAsync(int productId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkeyCoreDefaults.Cache.ProductExtensionByProductId, productId);
        await _staticCacheManager.RemoveAsync(key);
    }

    #endregion
}
