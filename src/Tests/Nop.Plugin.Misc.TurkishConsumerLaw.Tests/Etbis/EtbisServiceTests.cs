using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Etbis;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Etbis;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.Etbis;

/// <summary>
/// <see cref="EtbisService"/> birim testleri.
/// </summary>
[TestFixture]
public class EtbisServiceTests
{
    private Mock<IRepository<EtbisRegistration>> _repo = null!;
    private Mock<IStaticCacheManager> _cache = null!;
    private EtbisService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<EtbisRegistration>>();
        _cache = new Mock<IStaticCacheManager>();

        _cache.Setup(c => c.PrepareKeyForDefaultCache(It.IsAny<CacheKey>(), It.IsAny<object[]>()))
              .Returns<CacheKey, object[]>((key, args) => key.Create(o => o, args));

        _sut = new EtbisService(_repo.Object, _cache.Object);
    }

    private void SetupRepoReturns(IList<EtbisRegistration> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<EtbisRegistration>, IQueryable<EtbisRegistration>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    #region GetActiveAsync

    [Test]
    public async Task GetActiveAsync_StoreSpecificExists_ReturnsStoreSpecific()
    {
        var global = new EtbisRegistration { Id = 1, IsActive = true, LimitedToStoreId = 0, TradeName = "Global" };
        var store5 = new EtbisRegistration { Id = 2, IsActive = true, LimitedToStoreId = 5, TradeName = "Store5" };
        SetupRepoReturns(new[] { global, store5 });

        var result = await _sut.GetActiveAsync(storeId: 5);

        result.Should().Be(store5);
    }

    [Test]
    public async Task GetActiveAsync_NoStoreSpecific_FallsBackToGlobal()
    {
        var global = new EtbisRegistration { Id = 1, IsActive = true, LimitedToStoreId = 0, TradeName = "Global" };
        SetupRepoReturns(new[] { global });

        var result = await _sut.GetActiveAsync(storeId: 5);

        result.Should().Be(global);
    }

    [Test]
    public async Task GetActiveAsync_NoActive_ReturnsNull()
    {
        SetupRepoReturns(Array.Empty<EtbisRegistration>());

        var result = await _sut.GetActiveAsync(storeId: 5);

        result.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetByIdAsync_InvalidId_ReturnsNull(int id)
    {
        var result = await _sut.GetByIdAsync(id);
        result.Should().BeNull();
        _repo.Verify(r => r.GetByIdAsync(It.IsAny<int?>(), It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_ValidId_DelegatesToRepository()
    {
        var entity = new EtbisRegistration { Id = 5, TradeName = "Test" };
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
             .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(5);

        result.Should().Be(entity);
    }

    #endregion

    #region Insert/Update/Delete

    [Test]
    public async Task InsertAsync_SetsCreatedOnUtc_AndInvalidatesCache()
    {
        var entity = new EtbisRegistration { TradeName = "Test", MersisNo = "1234567890123456" };
        var beforeUtc = DateTime.UtcNow;

        await _sut.InsertAsync(entity);

        entity.CreatedOnUtc.Should().BeOnOrAfter(beforeUtc);
        _repo.Verify(r => r.InsertAsync(entity, It.IsAny<bool>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync(
            It.Is<CacheKey>(k => k.Key.Contains("Etbis.Active")),
            It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_SetsUpdatedOnUtc_AndInvalidatesCache()
    {
        var entity = new EtbisRegistration { Id = 1, TradeName = "Test" };
        var beforeUtc = DateTime.UtcNow;

        await _sut.UpdateAsync(entity);

        entity.UpdatedOnUtc.Should().NotBeNull();
        entity.UpdatedOnUtc!.Value.Should().BeOnOrAfter(beforeUtc);
        _repo.Verify(r => r.UpdateAsync(entity, It.IsAny<bool>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<CacheKey>(), It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_DelegatesAndInvalidatesCache()
    {
        var entity = new EtbisRegistration { Id = 1 };

        await _sut.DeleteAsync(entity);

        _repo.Verify(r => r.DeleteAsync(entity, It.IsAny<bool>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<CacheKey>(), It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public async Task InsertAsync_NullEntity_Throws()
    {
        var act = () => _sut.InsertAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion
}
