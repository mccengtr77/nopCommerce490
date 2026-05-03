using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.Kvkk;

[TestFixture]
public class ConsentRecordServiceTests
{
    private Mock<IRepository<ConsentRecord>> _repo = null!;
    private ConsentRecordService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<ConsentRecord>>();
        _sut = new ConsentRecordService(_repo.Object);
    }

    private void SetupRepoReturns(IList<ConsentRecord> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<ConsentRecord>, IQueryable<ConsentRecord>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    #region RecordAsync

    [Test]
    public async Task RecordAsync_StoresAllFields()
    {
        var record = await _sut.RecordAsync(
            customerId: 100,
            scope: ConsentScope.KvkkPersonalData,
            granted: true,
            textVersion: "1.0",
            source: ConsentSource.Registration,
            ipAddress: "10.0.0.1",
            userAgent: "Mozilla/5.0");

        record.CustomerId.Should().Be(100);
        record.Scope.Should().Be(ConsentScope.KvkkPersonalData);
        record.Granted.Should().BeTrue();
        record.TextVersion.Should().Be("1.0");
        record.Source.Should().Be(ConsentSource.Registration);
        record.IpAddress.Should().Be("10.0.0.1");
        record.UserAgent.Should().Be("Mozilla/5.0");
        record.CreatedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        record.ContentHash.Should().HaveLength(64);

        _repo.Verify(r => r.InsertAsync(record, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task RecordAsync_NullIpAndUa_StoresEmpty()
    {
        var record = await _sut.RecordAsync(
            1, ConsentScope.EtkSms, true, "1.0", ConsentSource.Registration,
            ipAddress: null, userAgent: null);

        record.IpAddress.Should().Be(string.Empty);
        record.UserAgent.Should().Be(string.Empty);
    }

    [Test]
    public async Task RecordAsync_NullTextVersion_StoresEmpty()
    {
        var record = await _sut.RecordAsync(
            1, ConsentScope.EtkSms, true, textVersion: null!, ConsentSource.Registration,
            "10.0.0.1", "ua");

        record.TextVersion.Should().Be(string.Empty);
    }

    #endregion

    #region RecordBatchAsync

    [Test]
    public async Task RecordBatchAsync_CreatesOneRecordPerScope()
    {
        var consents = new[]
        {
            (Scope: ConsentScope.KvkkPersonalData, Granted: true,  TextVersion: "1.0"),
            (Scope: ConsentScope.KvkkProfiling,    Granted: false, TextVersion: "1.0"),
            (Scope: ConsentScope.EtkSms,           Granted: true,  TextVersion: "1.0")
        };

        var records = await _sut.RecordBatchAsync(
            customerId: 100, consents,
            ConsentSource.Registration, "10.0.0.1", "ua");

        records.Should().HaveCount(3);
        records.Select(r => r.Scope).Should().BeEquivalentTo(new[]
        {
            ConsentScope.KvkkPersonalData, ConsentScope.KvkkProfiling, ConsentScope.EtkSms
        });
        records.All(r => r.CustomerId == 100).Should().BeTrue();
        records.All(r => r.Source == ConsentSource.Registration).Should().BeTrue();
        records.All(r => r.ContentHash.Length == 64).Should().BeTrue();

        _repo.Verify(r => r.InsertAsync(
            It.Is<IList<ConsentRecord>>(list => list.Count == 3),
            It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task RecordBatchAsync_MixedGranted_PreservesEachFlag()
    {
        var consents = new[]
        {
            (Scope: ConsentScope.KvkkPersonalData, Granted: true,  TextVersion: "1.0"),
            (Scope: ConsentScope.EtkSms,           Granted: false, TextVersion: "1.0")
        };

        var records = await _sut.RecordBatchAsync(100, consents, ConsentSource.Registration, null, null);

        records.Single(r => r.Scope == ConsentScope.KvkkPersonalData).Granted.Should().BeTrue();
        records.Single(r => r.Scope == ConsentScope.EtkSms).Granted.Should().BeFalse();
    }

    [Test]
    public async Task RecordBatchAsync_EmptyEnumerable_InsertsEmptyList()
    {
        var records = await _sut.RecordBatchAsync(
            100, Array.Empty<(ConsentScope, bool, string)>(),
            ConsentSource.Registration, null, null);

        records.Should().BeEmpty();
    }

    #endregion

    #region GetEffectiveAsync

    [Test]
    public async Task GetEffectiveAsync_MultipleHistoryEntries_ReturnsLatestPerScope()
    {
        var older = new ConsentRecord
        {
            Id = 1, CustomerId = 100, Scope = ConsentScope.KvkkPersonalData,
            Granted = true, CreatedOnUtc = DateTime.UtcNow.AddDays(-2)
        };
        var newer = new ConsentRecord
        {
            Id = 2, CustomerId = 100, Scope = ConsentScope.KvkkPersonalData,
            Granted = false, CreatedOnUtc = DateTime.UtcNow.AddDays(-1)
        };
        var differentScope = new ConsentRecord
        {
            Id = 3, CustomerId = 100, Scope = ConsentScope.EtkSms,
            Granted = true, CreatedOnUtc = DateTime.UtcNow
        };

        // Servis OrderByDescending(CreatedOnUtc) yaptığı için mock'tan
        // newest-first sırayla dönmeli
        SetupRepoReturns(new[] { differentScope, newer, older });

        var effective = await _sut.GetEffectiveAsync(100);

        effective.Should().HaveCount(2);
        effective[ConsentScope.KvkkPersonalData].Should().Be(newer);
        effective[ConsentScope.KvkkPersonalData].Granted.Should().BeFalse();  // Geri çekildi
        effective[ConsentScope.EtkSms].Should().Be(differentScope);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetEffectiveAsync_InvalidId_ReturnsEmpty(int id)
    {
        var result = await _sut.GetEffectiveAsync(id);
        result.Should().BeEmpty();
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetHistoryAsync_InvalidId_ReturnsEmpty(int id)
    {
        var result = await _sut.GetHistoryAsync(id);
        result.Should().BeEmpty();
    }

    #endregion

    #region ContentHash

    [Test]
    public void ComputeHash_DeterministicForSameInput()
    {
        var record = new ConsentRecord
        {
            CustomerId = 5,
            Scope = ConsentScope.KvkkPersonalData,
            Granted = true,
            TextVersion = "1.0",
            Source = ConsentSource.Registration,
            CreatedOnUtc = new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc)
        };

        var h1 = ConsentRecordService.ComputeHash(record);
        var h2 = ConsentRecordService.ComputeHash(record);

        h1.Should().Be(h2);
        h1.Should().HaveLength(64);
        h1.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Test]
    public void ComputeHash_DifferentGranted_DifferentHash()
    {
        var ts = new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc);
        var grantedRecord = new ConsentRecord
        {
            CustomerId = 5, Scope = ConsentScope.KvkkPersonalData, Granted = true,
            TextVersion = "1.0", Source = ConsentSource.Registration, CreatedOnUtc = ts
        };
        var deniedRecord = new ConsentRecord
        {
            CustomerId = 5, Scope = ConsentScope.KvkkPersonalData, Granted = false,
            TextVersion = "1.0", Source = ConsentSource.Registration, CreatedOnUtc = ts
        };

        ConsentRecordService.ComputeHash(grantedRecord)
            .Should().NotBe(ConsentRecordService.ComputeHash(deniedRecord));
    }

    #endregion
}
