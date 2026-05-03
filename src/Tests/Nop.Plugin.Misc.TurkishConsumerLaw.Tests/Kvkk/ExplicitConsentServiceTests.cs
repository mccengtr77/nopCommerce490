using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.Kvkk;

[TestFixture]
public class ExplicitConsentServiceTests
{
    private Mock<IRepository<ExplicitConsentText>> _repo = null!;
    private Mock<IStaticCacheManager> _cache = null!;
    private ExplicitConsentService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<ExplicitConsentText>>();
        _cache = new Mock<IStaticCacheManager>();
        _cache.Setup(c => c.PrepareKeyForDefaultCache(It.IsAny<CacheKey>(), It.IsAny<object[]>()))
              .Returns<CacheKey, object[]>((key, args) => key.Create(o => o, args));
        _sut = new ExplicitConsentService(_repo.Object, _cache.Object);
    }

    private void SetupRepoReturns(IList<ExplicitConsentText> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<ExplicitConsentText>, IQueryable<ExplicitConsentText>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    [TestCase(ConsentScope.KvkkPersonalData, true)]
    [TestCase(ConsentScope.KvkkProfiling, true)]
    [TestCase(ConsentScope.KvkkOverseasTransfer, true)]
    [TestCase(ConsentScope.EtkSms, false)]
    [TestCase(ConsentScope.EtkEmail, false)]
    [TestCase(ConsentScope.EtkCall, false)]
    public void IsKvkkScope_ClassifiesCorrectly(ConsentScope scope, bool expected)
    {
        ExplicitConsentService.IsKvkkScope(scope).Should().Be(expected);
        ExplicitConsentService.IsEtkScope(scope).Should().Be(!expected);
    }

    [Test]
    public async Task GetActiveKvkkAsync_FiltersOutEtkScopes()
    {
        SetupRepoReturns(new[]
        {
            new ExplicitConsentText { Scope = ConsentScope.KvkkPersonalData, ShortLabel = "kvkk1" },
            new ExplicitConsentText { Scope = ConsentScope.KvkkProfiling,    ShortLabel = "kvkk2" },
            new ExplicitConsentText { Scope = ConsentScope.EtkSms,           ShortLabel = "etk1"  }
        });

        var result = await _sut.GetActiveKvkkAsync(0);

        result.Should().HaveCount(2);
        result.All(t => ExplicitConsentService.IsKvkkScope(t.Scope)).Should().BeTrue();
    }

    [Test]
    public async Task GetActiveEtkAsync_FiltersOutKvkkScopes()
    {
        SetupRepoReturns(new[]
        {
            new ExplicitConsentText { Scope = ConsentScope.KvkkPersonalData, ShortLabel = "kvkk1" },
            new ExplicitConsentText { Scope = ConsentScope.EtkSms,           ShortLabel = "etk1"  },
            new ExplicitConsentText { Scope = ConsentScope.EtkEmail,         ShortLabel = "etk2"  }
        });

        var result = await _sut.GetActiveEtkAsync(0);

        result.Should().HaveCount(2);
        result.All(t => ExplicitConsentService.IsEtkScope(t.Scope)).Should().BeTrue();
    }

    [Test]
    public async Task GetActiveAsync_StoreSpecificOverridesGlobal()
    {
        SetupRepoReturns(new[]
        {
            new ExplicitConsentText { Id = 1, Scope = ConsentScope.KvkkPersonalData, LimitedToStoreId = 0, ShortLabel = "global", DisplayOrder = 1 },
            new ExplicitConsentText { Id = 2, Scope = ConsentScope.KvkkPersonalData, LimitedToStoreId = 5, ShortLabel = "store5", DisplayOrder = 1 }
        });

        var result = await _sut.GetActiveAsync(storeId: 5);

        result.Should().HaveCount(1);
        result[0].ShortLabel.Should().Be("store5");
    }

    [Test]
    public async Task GetByScopeAsync_FindsCorrectScope()
    {
        SetupRepoReturns(new[]
        {
            new ExplicitConsentText { Scope = ConsentScope.KvkkPersonalData, ShortLabel = "personal" },
            new ExplicitConsentText { Scope = ConsentScope.EtkSms, ShortLabel = "sms" }
        });

        var result = await _sut.GetByScopeAsync(ConsentScope.EtkSms, 0);

        result.Should().NotBeNull();
        result!.ShortLabel.Should().Be("sms");
    }
}
