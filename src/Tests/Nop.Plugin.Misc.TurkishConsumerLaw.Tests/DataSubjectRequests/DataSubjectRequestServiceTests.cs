using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.DataSubjectRequests;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.DataSubjectRequests;

[TestFixture]
public class DataSubjectRequestServiceTests
{
    private Mock<IRepository<DataSubjectRequest>> _repo = null!;
    private DataSubjectRequestService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<DataSubjectRequest>>();
        _sut = new DataSubjectRequestService(_repo.Object);
    }

    private void SetupRepoReturns(IList<DataSubjectRequest> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<DataSubjectRequest>, IQueryable<DataSubjectRequest>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    #region SubmitAsync

    [Test]
    public async Task SubmitAsync_SetsTimestampsAndStatus()
    {
        var request = new DataSubjectRequest
        {
            FullName = "Ahmet Yılmaz",
            Email = "ahmet@example.com",
            RequestType = DataSubjectRequestType.Erasure,
            Description = "Hesabımdaki tüm verileri silin.",
            // Test öncesi farklı status verelim, override olduğunu görelim
            Status = DataSubjectRequestStatus.Approved
        };

        var beforeUtc = DateTime.UtcNow;
        var saved = await _sut.SubmitAsync(request);

        saved.Status.Should().Be(DataSubjectRequestStatus.Submitted);
        saved.SubmittedOnUtc.Should().BeOnOrAfter(beforeUtc);
        saved.RespondedOnUtc.Should().BeNull();

        // 30 gün deadline kuralı (KVKK m.13/(2))
        saved.DeadlineUtc.Should().BeCloseTo(saved.SubmittedOnUtc.AddDays(30), TimeSpan.FromSeconds(1));

        _repo.Verify(r => r.InsertAsync(saved, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task SubmitAsync_NullEntity_Throws()
    {
        var act = () => _sut.SubmitAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task SubmitAsync_DeadlineConstantIsThirtyDays()
    {
        // KVKK m.13/(2) süre değiştiyse bu test bilinçli kırılır
        DataSubjectRequestService.LegalResponseDays.Should().Be(30);

        var saved = await _sut.SubmitAsync(new DataSubjectRequest
        {
            FullName = "X", Email = "x@y.z", RequestType = DataSubjectRequestType.Inquire,
            Description = "ABCDEFGHIJ"
        });

        (saved.DeadlineUtc - saved.SubmittedOnUtc).Days.Should().Be(30);
    }

    #endregion

    #region GetByCustomerAsync

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetByCustomerAsync_InvalidId_ReturnsEmpty(int id)
    {
        var result = await _sut.GetByCustomerAsync(id);
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetByCustomerAsync_ValidId_ReturnsFromRepo()
    {
        var data = new[] { new DataSubjectRequest { Id = 1, CustomerId = 100 } };
        SetupRepoReturns(data);

        var result = await _sut.GetByCustomerAsync(100);

        result.Should().HaveCount(1);
    }

    #endregion

    #region RespondAsync

    [Test]
    public async Task RespondAsync_Approved_SetsRespondedOn()
    {
        var existing = new DataSubjectRequest
        {
            Id = 5,
            FullName = "x", Email = "x@y.z",
            Status = DataSubjectRequestStatus.Submitted
        };
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
             .ReturnsAsync(existing);

        var beforeUtc = DateTime.UtcNow;
        await _sut.RespondAsync(5, DataSubjectRequestStatus.Approved, "Verileriniz silindi.");

        existing.Status.Should().Be(DataSubjectRequestStatus.Approved);
        existing.AdminResponse.Should().Be("Verileriniz silindi.");
        existing.RespondedOnUtc.Should().NotBeNull();
        existing.RespondedOnUtc!.Value.Should().BeOnOrAfter(beforeUtc);

        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task RespondAsync_InReview_DoesNotSetRespondedOn()
    {
        var existing = new DataSubjectRequest
        {
            Id = 5, FullName = "x", Email = "x@y.z",
            Status = DataSubjectRequestStatus.Submitted
        };
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
             .ReturnsAsync(existing);

        await _sut.RespondAsync(5, DataSubjectRequestStatus.InReview, "İncelemeye alındı.");

        existing.RespondedOnUtc.Should().BeNull();
    }

    [Test]
    public async Task RespondAsync_NonExisting_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int?>(), It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
             .ReturnsAsync((DataSubjectRequest?)null);

        var act = () => _sut.RespondAsync(999, DataSubjectRequestStatus.Approved, null);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region GetOverdueAsync

    [Test]
    public async Task GetOverdueAsync_DelegatesToRepo()
    {
        var overdue = new DataSubjectRequest
        {
            Id = 1, FullName = "x", Email = "x@y.z",
            DeadlineUtc = DateTime.UtcNow.AddDays(-5),
            RespondedOnUtc = null
        };
        SetupRepoReturns(new[] { overdue });

        var result = await _sut.GetOverdueAsync();

        result.Should().HaveCount(1);
    }

    #endregion
}
