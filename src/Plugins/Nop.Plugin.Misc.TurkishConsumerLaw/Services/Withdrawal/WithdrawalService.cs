using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;

/// <inheritdoc />
public class WithdrawalService : IWithdrawalService
{
    protected readonly IRepository<WithdrawalRequest> _repository;

    public WithdrawalService(IRepository<WithdrawalRequest> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public virtual async Task<WithdrawalRequest> SubmitAsync(WithdrawalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.SubmittedOnUtc = DateTime.UtcNow;
        request.Status = WithdrawalStatus.Pending;
        request.ApprovedOnUtc = null;
        request.ReceivedOnUtc = null;
        request.RefundedOnUtc = null;
        request.CompletedOnUtc = null;

        await _repository.InsertAsync(request);
        return request;
    }

    /// <inheritdoc />
    public virtual async Task<WithdrawalRequest?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<WithdrawalRequest?> GetByOrderIdAsync(int orderId)
    {
        if (orderId <= 0) return null;

        var list = await _repository.GetAllAsync(
            query => query.Where(w => w.OrderId == orderId)
                          .OrderByDescending(w => w.SubmittedOnUtc),
            getCacheKey: null);
        return list.FirstOrDefault();
    }

    /// <inheritdoc />
    public virtual async Task<IList<WithdrawalRequest>> GetByCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            return new List<WithdrawalRequest>();

        return await _repository.GetAllAsync(
            query => query.Where(w => w.CustomerId == customerId)
                          .OrderByDescending(w => w.SubmittedOnUtc),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task<IList<WithdrawalRequest>> GetByStatusAsync(WithdrawalStatus? status = null)
    {
        return await _repository.GetAllAsync(
            query => status is null
                ? query.OrderByDescending(w => w.SubmittedOnUtc)
                : query.Where(w => w.Status == status.Value).OrderByDescending(w => w.SubmittedOnUtc),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task ApproveAsync(int id)
    {
        var request = await RequireAsync(id);
        EnsureStatus(request, WithdrawalStatus.Pending);

        request.Status = WithdrawalStatus.Approved;
        request.ApprovedOnUtc = DateTime.UtcNow;
        await _repository.UpdateAsync(request);
    }

    /// <inheritdoc />
    public virtual async Task RejectAsync(int id, string rejectionReason)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Red gerekçesi boş olamaz.", nameof(rejectionReason));

        var request = await RequireAsync(id);
        EnsureStatus(request, WithdrawalStatus.Pending);

        request.Status = WithdrawalStatus.Rejected;
        request.RejectionReason = rejectionReason.Trim();
        await _repository.UpdateAsync(request);
    }

    /// <inheritdoc />
    public virtual async Task CancelByCustomerAsync(int id, int customerId)
    {
        var request = await RequireAsync(id);

        if (request.CustomerId != customerId)
            throw new InvalidOperationException("Bu talep size ait değil.");

        EnsureStatus(request, WithdrawalStatus.Pending);

        request.Status = WithdrawalStatus.Cancelled;
        await _repository.UpdateAsync(request);
    }

    /// <inheritdoc />
    public virtual async Task AdvanceAsync(int id, WithdrawalStatus newStatus, string? note = null, decimal? refundAmount = null)
    {
        var request = await RequireAsync(id);

        if (!IsValidTransition(request.Status, newStatus))
            throw new InvalidOperationException(
                $"Geçersiz durum geçişi: {request.Status} → {newStatus}");

        request.Status = newStatus;

        switch (newStatus)
        {
            case WithdrawalStatus.InTransit:
                request.ReturnTrackingNumber = note;
                break;
            case WithdrawalStatus.Received:
                request.ReceivedOnUtc = DateTime.UtcNow;
                break;
            case WithdrawalStatus.Inspected:
                request.InspectionNotes = note;
                break;
            case WithdrawalStatus.Refunded:
                request.RefundedOnUtc = DateTime.UtcNow;
                request.RefundAmount = refundAmount;
                break;
            case WithdrawalStatus.Completed:
                request.CompletedOnUtc = DateTime.UtcNow;
                break;
        }

        await _repository.UpdateAsync(request);
    }

    #region Helpers

    /// <summary>
    /// State machine geçişlerinin geçerlilik tablosu.
    /// Public static — testlerde doğrulanabilir.
    /// </summary>
    public static bool IsValidTransition(WithdrawalStatus from, WithdrawalStatus to) => (from, to) switch
    {
        (WithdrawalStatus.Pending, WithdrawalStatus.Approved)   => true,
        (WithdrawalStatus.Pending, WithdrawalStatus.Rejected)   => true,
        (WithdrawalStatus.Pending, WithdrawalStatus.Cancelled)  => true,
        (WithdrawalStatus.Approved, WithdrawalStatus.InTransit) => true,
        (WithdrawalStatus.InTransit, WithdrawalStatus.Received) => true,
        (WithdrawalStatus.Received, WithdrawalStatus.Inspected) => true,
        (WithdrawalStatus.Inspected, WithdrawalStatus.Refunded) => true,
        (WithdrawalStatus.Refunded, WithdrawalStatus.Completed) => true,
        _                                                       => false
    };

    private async Task<WithdrawalRequest> RequireAsync(int id)
    {
        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Cayma talebi {id} bulunamadı.");
    }

    private static void EnsureStatus(WithdrawalRequest request, WithdrawalStatus expected)
    {
        if (request.Status != expected)
            throw new InvalidOperationException(
                $"Bu işlem yalnızca {expected} durumundaki talepler için geçerlidir (mevcut: {request.Status}).");
    }

    #endregion
}
