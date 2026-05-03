using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.Cookies;

/// <summary>
/// <see cref="CookieDefinitionService"/> birim testleri.
/// </summary>
[TestFixture]
public class CookieDefinitionServiceTests
{
    private Mock<IRepository<CookieDefinition>> _repo = null!;
    private Mock<IStaticCacheManager> _cache = null!;
    private CookieDefinitionService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<CookieDefinition>>();
        _cache = new Mock<IStaticCacheManager>();
        _sut = new CookieDefinitionService(_repo.Object, _cache.Object);
    }

    private void SetupRepoReturns(IList<CookieDefinition> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<CookieDefinition>, IQueryable<CookieDefinition>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    [Test]
    public async Task GetByCategoryAsync_FiltersFromCachedList()
    {
        SetupRepoReturns(new[]
        {
            new CookieDefinition { Id = 1, Name = "_ga",  Category = CookieCategory.Analytics },
            new CookieDefinition { Id = 2, Name = "_fbp", Category = CookieCategory.Marketing },
            new CookieDefinition { Id = 3, Name = "_gid", Category = CookieCategory.Analytics }
        });

        var result = await _sut.GetByCategoryAsync(CookieCategory.Analytics);

        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().BeEquivalentTo("_ga", "_gid");
    }

    [Test]
    public async Task InsertAsync_InvalidatesCache()
    {
        var definition = new CookieDefinition { Name = "test" };

        await _sut.InsertAsync(definition);

        _repo.Verify(r => r.InsertAsync(definition, It.IsAny<bool>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync(
            It.Is<CacheKey>(k => k.Key.Contains("Cookies.Definitions.Active")),
            It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_InvalidatesCache()
    {
        var definition = new CookieDefinition { Id = 1, Name = "test" };
        await _sut.UpdateAsync(definition);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<CacheKey>(), It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_InvalidatesCache()
    {
        var definition = new CookieDefinition { Id = 1, Name = "test" };
        await _sut.DeleteAsync(definition);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<CacheKey>(), It.IsAny<object[]>()), Times.Once);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetByIdAsync_InvalidId_ReturnsNull(int id)
    {
        var result = await _sut.GetByIdAsync(id);
        result.Should().BeNull();
    }
}
