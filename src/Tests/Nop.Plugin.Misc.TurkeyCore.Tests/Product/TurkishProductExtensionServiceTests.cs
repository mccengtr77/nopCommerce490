using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using NopCurrencySettings = Nop.Core.Domain.Directory.CurrencySettings;
using Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;
using Nop.Plugin.Misc.TurkeyCore.Services.Product;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Logging;
using NUnit.Framework;
using NopProduct = Nop.Core.Domain.Catalog.Product;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.Product;

/// <summary>
/// <see cref="TurkishProductExtensionService"/> birim testleri (persisted recalc API).
/// IRepository, IStaticCacheManager, ICurrencyService, ITcmbExchangeRateService, IProductService, ILogger mock'lanır.
/// </summary>
[TestFixture]
public class TurkishProductExtensionServiceTests
{
    private Mock<IRepository<TurkishProductExtension>> _repo = null!;
    private Mock<IStaticCacheManager> _cache = null!;
    private Mock<ICurrencyService> _currencyService = null!;
    private Mock<ITcmbExchangeRateService> _tcmbService = null!;
    private Mock<IProductService> _productService = null!;
    private Mock<ILogger> _logger = null!;
    private TurkishProductExtensionService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<TurkishProductExtension>>();
        _cache = new Mock<IStaticCacheManager>();
        _currencyService = new Mock<ICurrencyService>();
        _tcmbService = new Mock<ITcmbExchangeRateService>();
        _productService = new Mock<IProductService>();
        _logger = new Mock<ILogger>();

        // Cache bypass — her çağrıda factory çalışır
        _cache.Setup(c => c.PrepareKeyForDefaultCache(It.IsAny<CacheKey>(), It.IsAny<object[]>()))
              .Returns<CacheKey, object[]>((key, args) => key);
        _cache.Setup(c => c.GetAsync(It.IsAny<CacheKey>(), It.IsAny<Func<Task<TurkishProductExtension?>>>()))
              .Returns<CacheKey, Func<Task<TurkishProductExtension?>>>(async (key, factory) => await factory());

        _sut = new TurkishProductExtensionService(_repo.Object, _cache.Object,
            _currencyService.Object, _tcmbService.Object,
            _productService.Object, _logger.Object,
            new NopCurrencySettings { PrimaryStoreCurrencyId = 1 });
    }

    private void SetupRepoReturns(IList<TurkishProductExtension> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<TurkishProductExtension>, IQueryable<TurkishProductExtension>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    #region GetByProductIdAsync

    [TestCase(0)]
    [TestCase(-5)]
    public async Task GetByProductIdAsync_InvalidId_ReturnsNullWithoutQuery(int id)
    {
        var result = await _sut.GetByProductIdAsync(id);
        result.Should().BeNull();
    }

    [Test]
    public async Task GetByProductIdAsync_ExistingProduct_ReturnsExtension()
    {
        var ext = new TurkishProductExtension { Id = 1, ProductId = 50, BaseCurrencyId = 2, BasePrice = 100m };
        SetupRepoReturns(new[] { ext });

        var result = await _sut.GetByProductIdAsync(50);

        result.Should().Be(ext);
    }

    [Test]
    public async Task GetByProductIdAsync_NoExtension_ReturnsNull()
    {
        SetupRepoReturns(Array.Empty<TurkishProductExtension>());

        var result = await _sut.GetByProductIdAsync(99);

        result.Should().BeNull();
    }

    #endregion

    #region UpsertAsync

    [Test]
    public async Task UpsertAsync_NullExtension_Throws()
    {
        var act = () => _sut.UpsertAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task UpsertAsync_InvalidProductId_Throws()
    {
        var ext = new TurkishProductExtension { ProductId = 0, BasePrice = 100m };
        var act = () => _sut.UpsertAsync(ext);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*ProductId*");
    }

    [Test]
    public async Task UpsertAsync_NewExtension_Inserts()
    {
        var ext = new TurkishProductExtension { Id = 0, ProductId = 50, BasePrice = 100m };

        await _sut.UpsertAsync(ext);

        _repo.Verify(r => r.InsertAsync(ext, It.IsAny<bool>()), Times.Once);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<TurkishProductExtension>(), It.IsAny<bool>()), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<CacheKey>(), It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public async Task UpsertAsync_ExistingExtension_Updates()
    {
        var ext = new TurkishProductExtension { Id = 7, ProductId = 50, BasePrice = 200m };

        await _sut.UpsertAsync(ext);

        _repo.Verify(r => r.UpdateAsync(ext, It.IsAny<bool>()), Times.Once);
        _repo.Verify(r => r.InsertAsync(It.IsAny<TurkishProductExtension>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion

    #region DeleteByProductIdAsync

    [TestCase(0)]
    [TestCase(-1)]
    public async Task DeleteByProductIdAsync_InvalidId_NoOp(int id)
    {
        await _sut.DeleteByProductIdAsync(id);

        _repo.Verify(r => r.DeleteAsync(It.IsAny<IList<TurkishProductExtension>>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task DeleteByProductIdAsync_ExistingExtension_DeletesAndInvalidatesCache()
    {
        var ext = new TurkishProductExtension { Id = 1, ProductId = 50 };
        SetupRepoReturns(new[] { ext });

        await _sut.DeleteByProductIdAsync(50);

        _repo.Verify(r => r.DeleteAsync(It.Is<IList<TurkishProductExtension>>(l => l.Contains(ext)), It.IsAny<bool>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<CacheKey>(), It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public async Task DeleteByProductIdAsync_NoExtension_DoesNotCallDelete()
    {
        SetupRepoReturns(Array.Empty<TurkishProductExtension>());

        await _sut.DeleteByProductIdAsync(50);

        _repo.Verify(r => r.DeleteAsync(It.IsAny<IList<TurkishProductExtension>>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion

    #region RecalculateAndPersistAsync

    [TestCase(0)]
    [TestCase(-1)]
    public async Task RecalculateAndPersistAsync_InvalidId_ReturnsFalse(int id)
    {
        var result = await _sut.RecalculateAndPersistAsync(id);

        result.Should().BeFalse();
        _productService.Verify(p => p.UpdateProductAsync(It.IsAny<NopProduct>()), Times.Never);
    }

    [Test]
    public async Task RecalculateAndPersistAsync_NoExtension_ReturnsFalseWithoutPersist()
    {
        SetupRepoReturns(Array.Empty<TurkishProductExtension>());

        var result = await _sut.RecalculateAndPersistAsync(50);

        result.Should().BeFalse();
        _productService.Verify(p => p.UpdateProductAsync(It.IsAny<NopProduct>()), Times.Never);
    }

    [Test]
    public async Task RecalculateAndPersistAsync_ExtensionWithoutCurrency_ReturnsFalseWithoutPersist()
    {
        SetupRepoReturns(new[] { new TurkishProductExtension { ProductId = 50, BaseCurrencyId = null, BasePrice = 100m } });

        var result = await _sut.RecalculateAndPersistAsync(50);

        result.Should().BeFalse();
        _productService.Verify(p => p.UpdateProductAsync(It.IsAny<NopProduct>()), Times.Never);
    }

    [Test]
    public async Task RecalculateAndPersistAsync_TcmbRateMissing_ReturnsFalseWithoutPersist()
    {
        SetupRepoReturns(new[] { new TurkishProductExtension { ProductId = 50, BaseCurrencyId = 2, BasePrice = 100m } });
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(2)).ReturnsAsync(new Currency { Id = 2, CurrencyCode = "USD" });
        _tcmbService.Setup(t => t.GetRateAsync("USD")).ReturnsAsync((decimal?)null);

        var result = await _sut.RecalculateAndPersistAsync(50);

        result.Should().BeFalse();
        _productService.Verify(p => p.UpdateProductAsync(It.IsAny<NopProduct>()), Times.Never);
    }

    [Test]
    public async Task RecalculateAndPersistAsync_ProductNotFound_ReturnsFalse()
    {
        SetupRepoReturns(new[] { new TurkishProductExtension { ProductId = 50, BaseCurrencyId = 2, BasePrice = 100m } });
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(2)).ReturnsAsync(new Currency { Id = 2, CurrencyCode = "USD" });
        _tcmbService.Setup(t => t.GetRateAsync("USD")).ReturnsAsync(33.5m);
        _productService.Setup(p => p.GetProductByIdAsync(50)).ReturnsAsync((NopProduct?)null);

        var result = await _sut.RecalculateAndPersistAsync(50);

        result.Should().BeFalse();
    }

    [Test]
    public async Task RecalculateAndPersistAsync_HappyPath_PersistsAllThreeFields()
    {
        var product = new NopProduct { Id = 50, Price = 0m, OldPrice = 0m, ProductCost = 0m };
        SetupRepoReturns(new[] { new TurkishProductExtension
        {
            ProductId = 50, BaseCurrencyId = 2,
            BasePrice = 100m, BaseOldPrice = 120m, BaseProductCost = 80m
        } });
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(2)).ReturnsAsync(new Currency { Id = 2, CurrencyCode = "USD" });
        _tcmbService.Setup(t => t.GetRateAsync("USD")).ReturnsAsync(33.5m);
        _productService.Setup(p => p.GetProductByIdAsync(50)).ReturnsAsync(product);

        var result = await _sut.RecalculateAndPersistAsync(50);

        result.Should().BeTrue();
        product.Price.Should().Be(3350m);
        product.OldPrice.Should().Be(4020m);
        product.ProductCost.Should().Be(2680m);
        _productService.Verify(p => p.UpdateProductAsync(product), Times.Once);
    }

    [Test]
    public async Task RecalculateAndPersistAsync_TryCurrency_PersistsIdentity()
    {
        var product = new NopProduct { Id = 50, Price = 0m };
        SetupRepoReturns(new[] { new TurkishProductExtension
        {
            ProductId = 50, BaseCurrencyId = 1, BasePrice = 999m
        } });
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(1)).ReturnsAsync(new Currency { Id = 1, CurrencyCode = "TRY" });
        _productService.Setup(p => p.GetProductByIdAsync(50)).ReturnsAsync(product);

        var result = await _sut.RecalculateAndPersistAsync(50);

        result.Should().BeTrue();
        product.Price.Should().Be(999m);
        _tcmbService.Verify(t => t.GetRateAsync(It.IsAny<string>()), Times.Never);
        _productService.Verify(p => p.UpdateProductAsync(product), Times.Once);
    }

    [Test]
    public async Task RecalculateAndPersistAsync_NullOldPriceAndCost_DoesNotMutateThem()
    {
        var product = new NopProduct { Id = 50, Price = 0m, OldPrice = 50m, ProductCost = 25m };
        SetupRepoReturns(new[] { new TurkishProductExtension
        {
            ProductId = 50, BaseCurrencyId = 2, BasePrice = 100m,
            BaseOldPrice = null, BaseProductCost = null
        } });
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(2)).ReturnsAsync(new Currency { Id = 2, CurrencyCode = "USD" });
        _tcmbService.Setup(t => t.GetRateAsync("USD")).ReturnsAsync(33m);
        _productService.Setup(p => p.GetProductByIdAsync(50)).ReturnsAsync(product);

        var result = await _sut.RecalculateAndPersistAsync(50);

        result.Should().BeTrue();
        product.Price.Should().Be(3300m);
        product.OldPrice.Should().Be(50m, "BaseOldPrice null → orijinal değer korunur");
        product.ProductCost.Should().Be(25m, "BaseProductCost null → orijinal değer korunur");
        _productService.Verify(p => p.UpdateProductAsync(product), Times.Once);
    }

    #endregion

    #region RecalculateAllAsync

    [Test]
    public async Task RecalculateAllAsync_NoExtensions_ReturnsZero()
    {
        SetupRepoReturns(Array.Empty<TurkishProductExtension>());

        var count = await _sut.RecalculateAllAsync();

        count.Should().Be(0);
        _productService.Verify(p => p.UpdateProductAsync(It.IsAny<NopProduct>()), Times.Never);
    }

    [Test]
    public async Task RecalculateAllAsync_ProcessesEachExtensionAndCountsSuccess()
    {
        var ext1 = new TurkishProductExtension { ProductId = 50, BaseCurrencyId = 2, BasePrice = 100m };
        var ext2 = new TurkishProductExtension { ProductId = 51, BaseCurrencyId = 3, BasePrice = 50m };
        SetupRepoReturns(new[] { ext1, ext2 });

        _currencyService.Setup(c => c.GetCurrencyByIdAsync(2)).ReturnsAsync(new Currency { Id = 2, CurrencyCode = "USD" });
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(3)).ReturnsAsync(new Currency { Id = 3, CurrencyCode = "EUR" });
        _tcmbService.Setup(t => t.GetRateAsync("USD")).ReturnsAsync(33m);
        _tcmbService.Setup(t => t.GetRateAsync("EUR")).ReturnsAsync(36m);
        _productService.Setup(p => p.GetProductByIdAsync(50)).ReturnsAsync(new NopProduct { Id = 50 });
        _productService.Setup(p => p.GetProductByIdAsync(51)).ReturnsAsync(new NopProduct { Id = 51 });

        var count = await _sut.RecalculateAllAsync();

        count.Should().Be(2);
        _productService.Verify(p => p.UpdateProductAsync(It.IsAny<NopProduct>()), Times.Exactly(2));
    }

    [Test]
    public async Task RecalculateAllAsync_FailedItemDoesNotBlockOthers()
    {
        var ext1 = new TurkishProductExtension { ProductId = 50, BaseCurrencyId = 2, BasePrice = 100m };
        var ext2 = new TurkishProductExtension { ProductId = 51, BaseCurrencyId = 3, BasePrice = 50m };
        SetupRepoReturns(new[] { ext1, ext2 });

        _currencyService.Setup(c => c.GetCurrencyByIdAsync(2)).ReturnsAsync(new Currency { Id = 2, CurrencyCode = "USD" });
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(3)).ReturnsAsync(new Currency { Id = 3, CurrencyCode = "EUR" });
        _tcmbService.Setup(t => t.GetRateAsync("USD")).ReturnsAsync(33m);
        _tcmbService.Setup(t => t.GetRateAsync("EUR")).ReturnsAsync(36m);
        _productService.Setup(p => p.GetProductByIdAsync(50)).ThrowsAsync(new InvalidOperationException("DB error"));
        _productService.Setup(p => p.GetProductByIdAsync(51)).ReturnsAsync(new NopProduct { Id = 51 });

        var count = await _sut.RecalculateAllAsync();

        count.Should().Be(1, "ext1 hata verdi, ext2 başarılı");
        _productService.Verify(p => p.UpdateProductAsync(It.Is<NopProduct>(pr => pr.Id == 51)), Times.Once);
    }

    #endregion

    #region ConvertBasePriceToTryAsync

    [Test]
    public async Task ConvertBasePriceToTryAsync_NoCurrency_ReturnsNull()
    {
        var ext = new TurkishProductExtension { ProductId = 50, BaseCurrencyId = null, BasePrice = 100m };

        var result = await _sut.ConvertBasePriceToTryAsync(ext);

        result.Should().BeNull();
    }

    [Test]
    public async Task ConvertBasePriceToTryAsync_HappyPath_ReturnsConvertedPrice()
    {
        var ext = new TurkishProductExtension { ProductId = 50, BaseCurrencyId = 3, BasePrice = 50m };
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(3)).ReturnsAsync(new Currency { Id = 3, CurrencyCode = "EUR" });
        _tcmbService.Setup(t => t.GetRateAsync("EUR")).ReturnsAsync(36.20m);

        var result = await _sut.ConvertBasePriceToTryAsync(ext);

        result.Should().Be(1810m);
    }

    [Test]
    public async Task ConvertBasePriceToTryAsync_RateMissing_ReturnsNull()
    {
        var ext = new TurkishProductExtension { ProductId = 50, BaseCurrencyId = 4, BasePrice = 50m };
        _currencyService.Setup(c => c.GetCurrencyByIdAsync(4)).ReturnsAsync(new Currency { Id = 4, CurrencyCode = "JPY" });
        _tcmbService.Setup(t => t.GetRateAsync("JPY")).ReturnsAsync((decimal?)null);

        var result = await _sut.ConvertBasePriceToTryAsync(ext);

        result.Should().BeNull();
    }

    #endregion
}
