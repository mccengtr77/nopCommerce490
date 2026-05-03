using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.Withdrawal;

[TestFixture]
public class WithdrawalServiceTests
{
    private Mock<IRepository<WithdrawalRequest>> _repo = null!;
    private WithdrawalService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<WithdrawalRequest>>();
        _sut = new WithdrawalService(_repo.Object);
    }

    private WithdrawalRequest StubRequest(int id, WithdrawalStatus status, int customerId = 100)
    {
        var req = new WithdrawalRequest
        {
            Id = id, OrderId = 50, CustomerId = customerId, Status = status,
            Description = "test", SubmittedOnUtc = DateTime.UtcNow.AddDays(-1)
        };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
             .ReturnsAsync(req);
        return req;
    }

    #region SubmitAsync

    [Test]
    public async Task SubmitAsync_SetsDefaults()
    {
        var input = new WithdrawalRequest
        {
            OrderId = 50, CustomerId = 100, Description = "iade",
            // Caller'ın yanlışlıkla farklı status gönderdiği durumda override eder
            Status = WithdrawalStatus.Approved
        };

        var beforeUtc = DateTime.UtcNow;
        var saved = await _sut.SubmitAsync(input);

        saved.Status.Should().Be(WithdrawalStatus.Pending);
        saved.SubmittedOnUtc.Should().BeOnOrAfter(beforeUtc);
        saved.ApprovedOnUtc.Should().BeNull();
        _repo.Verify(r => r.InsertAsync(saved, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task SubmitAsync_NullEntity_Throws()
    {
        var act = () => _sut.SubmitAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region ApproveAsync

    [Test]
    public async Task ApproveAsync_PendingRequest_TransitionsToApproved()
    {
        var req = StubRequest(1, WithdrawalStatus.Pending);

        var beforeUtc = DateTime.UtcNow;
        await _sut.ApproveAsync(1);

        req.Status.Should().Be(WithdrawalStatus.Approved);
        req.ApprovedOnUtc.Should().BeOnOrAfter(beforeUtc);
        _repo.Verify(r => r.UpdateAsync(req, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task ApproveAsync_NonPending_Throws()
    {
        StubRequest(1, WithdrawalStatus.Approved);

        var act = () => _sut.ApproveAsync(1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ApproveAsync_NotFound_Throws()
    {
        var act = () => _sut.ApproveAsync(999);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region RejectAsync

    [Test]
    public async Task RejectAsync_PendingRequest_StoresReasonAndTransitions()
    {
        var req = StubRequest(1, WithdrawalStatus.Pending);

        await _sut.RejectAsync(1, "Cayma süresi dışı");

        req.Status.Should().Be(WithdrawalStatus.Rejected);
        req.RejectionReason.Should().Be("Cayma süresi dışı");
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public async Task RejectAsync_EmptyReason_Throws(string? reason)
    {
        StubRequest(1, WithdrawalStatus.Pending);

        var act = () => _sut.RejectAsync(1, reason!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region CancelByCustomerAsync

    [Test]
    public async Task CancelByCustomerAsync_OwnerAndPending_Cancels()
    {
        var req = StubRequest(1, WithdrawalStatus.Pending, customerId: 100);

        await _sut.CancelByCustomerAsync(1, 100);

        req.Status.Should().Be(WithdrawalStatus.Cancelled);
    }

    [Test]
    public async Task CancelByCustomerAsync_DifferentCustomer_Throws()
    {
        StubRequest(1, WithdrawalStatus.Pending, customerId: 100);

        var act = () => _sut.CancelByCustomerAsync(1, 999);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task CancelByCustomerAsync_NonPending_Throws()
    {
        StubRequest(1, WithdrawalStatus.Approved, customerId: 100);

        var act = () => _sut.CancelByCustomerAsync(1, 100);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region AdvanceAsync

    [Test]
    public async Task AdvanceAsync_ApprovedToInTransit_SetsTrackingNumber()
    {
        var req = StubRequest(1, WithdrawalStatus.Approved);

        await _sut.AdvanceAsync(1, WithdrawalStatus.InTransit, note: "ARAS123");

        req.Status.Should().Be(WithdrawalStatus.InTransit);
        req.ReturnTrackingNumber.Should().Be("ARAS123");
    }

    [Test]
    public async Task AdvanceAsync_InTransitToReceived_SetsReceivedTimestamp()
    {
        var req = StubRequest(1, WithdrawalStatus.InTransit);

        var beforeUtc = DateTime.UtcNow;
        await _sut.AdvanceAsync(1, WithdrawalStatus.Received);

        req.Status.Should().Be(WithdrawalStatus.Received);
        req.ReceivedOnUtc.Should().BeOnOrAfter(beforeUtc);
    }

    [Test]
    public async Task AdvanceAsync_InspectedToRefunded_SetsAmountAndTimestamp()
    {
        var req = StubRequest(1, WithdrawalStatus.Inspected);

        var beforeUtc = DateTime.UtcNow;
        await _sut.AdvanceAsync(1, WithdrawalStatus.Refunded, refundAmount: 250.50m);

        req.Status.Should().Be(WithdrawalStatus.Refunded);
        req.RefundAmount.Should().Be(250.50m);
        req.RefundedOnUtc.Should().BeOnOrAfter(beforeUtc);
    }

    [Test]
    public async Task AdvanceAsync_RefundedToCompleted_SetsCompletedTimestamp()
    {
        var req = StubRequest(1, WithdrawalStatus.Refunded);

        await _sut.AdvanceAsync(1, WithdrawalStatus.Completed);

        req.Status.Should().Be(WithdrawalStatus.Completed);
        req.CompletedOnUtc.Should().NotBeNull();
    }

    [Test]
    public async Task AdvanceAsync_InvalidTransition_Throws()
    {
        StubRequest(1, WithdrawalStatus.Pending);

        // Pending → Refunded geçersiz
        var act = () => _sut.AdvanceAsync(1, WithdrawalStatus.Refunded);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region IsValidTransition

    [TestCase(WithdrawalStatus.Pending, WithdrawalStatus.Approved, true)]
    [TestCase(WithdrawalStatus.Pending, WithdrawalStatus.Rejected, true)]
    [TestCase(WithdrawalStatus.Pending, WithdrawalStatus.Cancelled, true)]
    [TestCase(WithdrawalStatus.Approved, WithdrawalStatus.InTransit, true)]
    [TestCase(WithdrawalStatus.InTransit, WithdrawalStatus.Received, true)]
    [TestCase(WithdrawalStatus.Received, WithdrawalStatus.Inspected, true)]
    [TestCase(WithdrawalStatus.Inspected, WithdrawalStatus.Refunded, true)]
    [TestCase(WithdrawalStatus.Refunded, WithdrawalStatus.Completed, true)]
    public void IsValidTransition_HappyPath_ReturnsTrue(WithdrawalStatus from, WithdrawalStatus to, bool expected)
    {
        WithdrawalService.IsValidTransition(from, to).Should().Be(expected);
    }

    [TestCase(WithdrawalStatus.Pending, WithdrawalStatus.InTransit)]
    [TestCase(WithdrawalStatus.Approved, WithdrawalStatus.Refunded)]
    [TestCase(WithdrawalStatus.Refunded, WithdrawalStatus.Approved)]      // backward
    [TestCase(WithdrawalStatus.Completed, WithdrawalStatus.Refunded)]     // terminal'dan geri
    [TestCase(WithdrawalStatus.Rejected, WithdrawalStatus.Approved)]      // rejected terminal
    [TestCase(WithdrawalStatus.Cancelled, WithdrawalStatus.Approved)]     // cancelled terminal
    public void IsValidTransition_InvalidPaths_ReturnsFalse(WithdrawalStatus from, WithdrawalStatus to)
    {
        WithdrawalService.IsValidTransition(from, to).Should().BeFalse();
    }

    #endregion
}
