using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.Cookies;

/// <summary>
/// <see cref="CookieConsentService"/> birim testleri.
/// </summary>
[TestFixture]
public class CookieConsentServiceTests
{
    private Mock<IRepository<CookieConsent>> _repo = null!;
    private CookieConsentService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<CookieConsent>>();
        _sut = new CookieConsentService(_repo.Object);
    }

    private void SetupRepoReturns(IList<CookieConsent> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<CookieConsent>, IQueryable<CookieConsent>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    #region RecordConsentAsync

    [Test]
    public async Task RecordConsentAsync_StoresAllFields()
    {
        var guid = Guid.NewGuid();

        var record = await _sut.RecordConsentAsync(
            guid, customerId: 100,
            functional: true, analytics: true, marketing: false,
            ipAddress: "10.0.0.1", userAgent: "Mozilla/5.0");

        record.ConsentGuid.Should().Be(guid);
        record.CustomerId.Should().Be(100);
        record.FunctionalAllowed.Should().BeTrue();
        record.AnalyticsAllowed.Should().BeTrue();
        record.MarketingAllowed.Should().BeFalse();
        record.IpAddress.Should().Be("10.0.0.1");
        record.UserAgent.Should().Be("Mozilla/5.0");
        record.PolicyVersion.Should().Be(TurkishConsumerLawDefaults.Cookies.CurrentPolicyVersion);
        record.AcceptedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        record.ContentHash.Should().NotBeNullOrEmpty();
        record.ContentHash.Length.Should().Be(64);  // SHA-256 hex

        _repo.Verify(r => r.InsertAsync(record, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task RecordConsentAsync_EmptyGuid_GeneratesNew()
    {
        var record = await _sut.RecordConsentAsync(
            Guid.Empty, customerId: 0,
            functional: false, analytics: false, marketing: false,
            ipAddress: null, userAgent: null);

        record.ConsentGuid.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task RecordConsentAsync_NullIpAndUa_StoresEmpty()
    {
        var record = await _sut.RecordConsentAsync(
            Guid.NewGuid(), customerId: 0,
            functional: false, analytics: false, marketing: false,
            ipAddress: null, userAgent: null);

        record.IpAddress.Should().Be(string.Empty);
        record.UserAgent.Should().Be(string.Empty);
    }

    [Test]
    public async Task RecordConsentAsync_DifferentChoices_ProduceDifferentHashes()
    {
        var guid = Guid.NewGuid();

        var allYes = await _sut.RecordConsentAsync(guid, 0, true, true, true, null, null);
        var allNo = await _sut.RecordConsentAsync(guid, 0, false, false, false, null, null);

        allYes.ContentHash.Should().NotBe(allNo.ContentHash);
    }

    #endregion

    #region GetLatestAsync

    [Test]
    public async Task GetLatestAsync_EmptyGuid_ReturnsNull()
    {
        var result = await _sut.GetLatestAsync(Guid.Empty);
        result.Should().BeNull();
    }

    [Test]
    public async Task GetLatestAsync_FoundRecords_ReturnsFirstFromOrderedQuery()
    {
        // Repo'nun içindeki OrderByDescending mock tarafından uygulanmaz;
        // mock list[0]'ı döner. Servisin contract'ı: .FirstOrDefault sonucu.
        var guid = Guid.NewGuid();
        var newest = new CookieConsent { Id = 2, ConsentGuid = guid, AcceptedOnUtc = DateTime.UtcNow };
        var older = new CookieConsent { Id = 1, ConsentGuid = guid, AcceptedOnUtc = DateTime.UtcNow.AddDays(-1) };
        SetupRepoReturns(new[] { newest, older });

        var result = await _sut.GetLatestAsync(guid);

        result.Should().Be(newest);
    }

    [Test]
    public async Task GetLatestAsync_NoRecords_ReturnsNull()
    {
        SetupRepoReturns(Array.Empty<CookieConsent>());

        var result = await _sut.GetLatestAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region GetLatestForCustomerAsync

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetLatestForCustomerAsync_InvalidId_ReturnsNull(int id)
    {
        var result = await _sut.GetLatestForCustomerAsync(id);
        result.Should().BeNull();
    }

    [Test]
    public async Task GetLatestForCustomerAsync_ValidId_ReturnsFromRepo()
    {
        var record = new CookieConsent { Id = 1, CustomerId = 100 };
        SetupRepoReturns(new[] { record });

        var result = await _sut.GetLatestForCustomerAsync(100);

        result.Should().Be(record);
    }

    #endregion

    #region ContentHash

    [Test]
    public void ComputeHash_DeterministicForSameInput()
    {
        var record = new CookieConsent
        {
            ConsentGuid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CustomerId = 5,
            FunctionalAllowed = true,
            AnalyticsAllowed = false,
            MarketingAllowed = true,
            PolicyVersion = "1.0",
            AcceptedOnUtc = new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc)
        };

        var h1 = CookieConsentService.ComputeHash(record);
        var h2 = CookieConsentService.ComputeHash(record);

        h1.Should().Be(h2);
        h1.Should().HaveLength(64);
        h1.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    #endregion
}
