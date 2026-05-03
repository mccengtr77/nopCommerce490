using System.Security.Cryptography;
using System.Text;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;

/// <inheritdoc />
public class ConsentRecordService : IConsentRecordService
{
    protected readonly IRepository<ConsentRecord> _repository;

    public ConsentRecordService(IRepository<ConsentRecord> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public virtual async Task<ConsentRecord> RecordAsync(
        int customerId,
        ConsentScope scope,
        bool granted,
        string textVersion,
        ConsentSource source,
        string? ipAddress,
        string? userAgent)
    {
        var record = new ConsentRecord
        {
            CustomerId = customerId,
            Scope = scope,
            Granted = granted,
            TextVersion = textVersion ?? string.Empty,
            Source = source,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = userAgent ?? string.Empty,
            CreatedOnUtc = DateTime.UtcNow
        };

        record.ContentHash = ComputeHash(record);

        await _repository.InsertAsync(record);
        return record;
    }

    /// <inheritdoc />
    public virtual async Task<IList<ConsentRecord>> RecordBatchAsync(
        int customerId,
        IEnumerable<(ConsentScope Scope, bool Granted, string TextVersion)> consents,
        ConsentSource source,
        string? ipAddress,
        string? userAgent)
    {
        ArgumentNullException.ThrowIfNull(consents);

        var now = DateTime.UtcNow;
        var records = consents.Select(c => new ConsentRecord
        {
            CustomerId = customerId,
            Scope = c.Scope,
            Granted = c.Granted,
            TextVersion = c.TextVersion ?? string.Empty,
            Source = source,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = userAgent ?? string.Empty,
            CreatedOnUtc = now
        }).ToList();

        foreach (var r in records)
            r.ContentHash = ComputeHash(r);

        await _repository.InsertAsync(records);
        return records;
    }

    /// <inheritdoc />
    public virtual async Task<IDictionary<ConsentScope, ConsentRecord>> GetEffectiveAsync(int customerId)
    {
        if (customerId <= 0)
            return new Dictionary<ConsentScope, ConsentRecord>();

        var all = await _repository.GetAllAsync(
            query => query.Where(c => c.CustomerId == customerId)
                          .OrderByDescending(c => c.CreatedOnUtc),
            getCacheKey: null);

        // Scope başına en güncel kayıt
        return all
            .GroupBy(c => c.Scope)
            .ToDictionary(g => g.Key, g => g.First());
    }

    /// <inheritdoc />
    public virtual async Task<IList<ConsentRecord>> GetHistoryAsync(int customerId)
    {
        if (customerId <= 0)
            return new List<ConsentRecord>();

        return await _repository.GetAllAsync(
            query => query.Where(c => c.CustomerId == customerId)
                          .OrderByDescending(c => c.CreatedOnUtc),
            getCacheKey: null);
    }

    /// <summary>
    /// SHA-256 hash — onay kaydı manipülasyon kontrolü.
    /// </summary>
    public static string ComputeHash(ConsentRecord r)
    {
        var payload = $"{r.CustomerId}|{(int)r.Scope}|{r.Granted}|{r.TextVersion}|{(int)r.Source}|{r.CreatedOnUtc:O}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
