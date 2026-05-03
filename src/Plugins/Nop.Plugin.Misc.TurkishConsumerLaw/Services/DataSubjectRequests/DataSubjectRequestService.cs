using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.DataSubjectRequests;

/// <inheritdoc />
public class DataSubjectRequestService : IDataSubjectRequestService
{
    /// <summary>
    /// KVKK m.13/(2) — başvuru en geç 30 gün içinde sonuçlandırılır.
    /// </summary>
    public const int LegalResponseDays = 30;

    protected readonly IRepository<DataSubjectRequest> _repository;

    public DataSubjectRequestService(IRepository<DataSubjectRequest> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public virtual async Task<DataSubjectRequest> SubmitAsync(DataSubjectRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = DateTime.UtcNow;
        request.SubmittedOnUtc = now;
        request.DeadlineUtc = now.AddDays(LegalResponseDays);
        request.Status = DataSubjectRequestStatus.Submitted;
        request.RespondedOnUtc = null;

        await _repository.InsertAsync(request);
        return request;
    }

    /// <inheritdoc />
    public virtual async Task<DataSubjectRequest?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<IList<DataSubjectRequest>> GetByCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            return new List<DataSubjectRequest>();

        return await _repository.GetAllAsync(
            query => query.Where(r => r.CustomerId == customerId)
                          .OrderByDescending(r => r.SubmittedOnUtc),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task<IList<DataSubjectRequest>> GetByStatusAsync(DataSubjectRequestStatus? status = null)
    {
        return await _repository.GetAllAsync(
            query => status is null
                ? query.OrderByDescending(r => r.SubmittedOnUtc)
                : query.Where(r => r.Status == status.Value).OrderByDescending(r => r.SubmittedOnUtc),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task RespondAsync(int requestId, DataSubjectRequestStatus newStatus, string? adminResponse)
    {
        var request = await GetByIdAsync(requestId)
            ?? throw new InvalidOperationException($"DSR {requestId} bulunamadı");

        request.Status = newStatus;
        request.AdminResponse = adminResponse;

        // Approved/Rejected → kapanmış sayılır, sayaç durur
        if (newStatus is DataSubjectRequestStatus.Approved
            or DataSubjectRequestStatus.Rejected
            or DataSubjectRequestStatus.Withdrawn)
        {
            request.RespondedOnUtc = DateTime.UtcNow;
        }

        await _repository.UpdateAsync(request);
    }

    /// <inheritdoc />
    public virtual async Task<IList<DataSubjectRequest>> GetOverdueAsync()
    {
        var now = DateTime.UtcNow;

        return await _repository.GetAllAsync(
            query => query
                .Where(r => r.RespondedOnUtc == null && r.DeadlineUtc < now)
                .OrderBy(r => r.DeadlineUtc),
            getCacheKey: null);
    }
}
