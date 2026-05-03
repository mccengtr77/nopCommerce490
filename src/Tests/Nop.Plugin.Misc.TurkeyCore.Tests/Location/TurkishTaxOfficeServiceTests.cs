using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.TaxOffice;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.Location;

/// <summary>
/// <see cref="TurkishTaxOfficeService"/> birim testleri.
/// </summary>
[TestFixture]
public class TurkishTaxOfficeServiceTests
{
    private Mock<IRepository<TurkishTaxOffice>> _repo = null!;
    private Mock<IStaticCacheManager> _cache = null!;
    private TurkishTaxOfficeService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<TurkishTaxOffice>>();
        _cache = new Mock<IStaticCacheManager>();
        _cache.Setup(c => c.PrepareKeyForDefaultCache(It.IsAny<CacheKey>(), It.IsAny<object[]>()))
              .Returns<CacheKey, object[]>((key, args) => key.Create(o => o, args));

        _sut = new TurkishTaxOfficeService(_repo.Object, _cache.Object);
    }

    private void SetupRepoReturns(IList<TurkishTaxOffice> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<TurkishTaxOffice>, IQueryable<TurkishTaxOffice>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    [Test]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var data = new List<TurkishTaxOffice>
        {
            new() { Id = 1, ProvinceId = 34, Name = "Beyoğlu V.D.", Code = "0660" }
        };
        SetupRepoReturns(data);

        var result = await _sut.GetAllAsync();

        result.Should().BeEquivalentTo(data);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetByProvinceIdAsync_InvalidId_ReturnsEmpty(int provinceId)
    {
        var result = await _sut.GetByProvinceIdAsync(provinceId);
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetByCodeAsync_FindsByCodeFromCachedList()
    {
        var data = new List<TurkishTaxOffice>
        {
            new() { Id = 1, ProvinceId = 34, Name = "Beyoğlu V.D.", Code = "0660" },
            new() { Id = 2, ProvinceId = 34, Name = "Kadıköy V.D.", Code = "0670" }
        };
        SetupRepoReturns(data);

        var result = await _sut.GetByCodeAsync("0670");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Kadıköy V.D.");
    }

    [Test]
    public async Task GetByCodeAsync_TrimsInput()
    {
        var data = new List<TurkishTaxOffice>
        {
            new() { Id = 1, ProvinceId = 34, Name = "Beyoğlu V.D.", Code = "0660" }
        };
        SetupRepoReturns(data);

        var result = await _sut.GetByCodeAsync("  0660  ");

        result.Should().NotBeNull();
    }

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public async Task GetByCodeAsync_NullOrEmpty_ReturnsNull(string? code)
    {
        var result = await _sut.GetByCodeAsync(code!);
        result.Should().BeNull();
    }

    [Test]
    public void InsertAsync_NullEntity_Throws()
    {
        var act = () => _sut.InsertAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task InsertAsync_DelegatesToRepository()
    {
        var entity = new TurkishTaxOffice { Name = "Test V.D.", Code = "9999", ProvinceId = 34 };

        await _sut.InsertAsync(entity);

        _repo.Verify(r => r.InsertAsync(entity, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_DelegatesToRepository()
    {
        var entity = new TurkishTaxOffice { Id = 1, Name = "Güncel V.D.", Code = "1234", ProvinceId = 6 };

        await _sut.UpdateAsync(entity);

        _repo.Verify(r => r.UpdateAsync(entity, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var entity = new TurkishTaxOffice { Id = 1 };

        await _sut.DeleteAsync(entity);

        _repo.Verify(r => r.DeleteAsync(entity, It.IsAny<bool>()), Times.Once);
    }
}
