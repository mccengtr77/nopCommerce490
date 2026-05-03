using System.Security.Cryptography;
using System.Text;
using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;

/// <inheritdoc />
public class CookieConsentService : ICookieConsentService
{
    #region Fields

    protected readonly IRepository<CookieConsent> _repository;

    #endregion

    #region Ctor

    public CookieConsentService(IRepository<CookieConsent> repository)
    {
        _repository = repository;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual async Task<CookieConsent> RecordConsentAsync(
        Guid consentGuid,
        int customerId,
        bool functional,
        bool analytics,
        bool marketing,
        string? ipAddress,
        string? userAgent)
    {
        if (consentGuid == Guid.Empty)
            consentGuid = Guid.NewGuid();

        var record = new CookieConsent
        {
            ConsentGuid = consentGuid,
            CustomerId = customerId,
            FunctionalAllowed = functional,
            AnalyticsAllowed = analytics,
            MarketingAllowed = marketing,
            PolicyVersion = TurkishConsumerLawDefaults.Cookies.CurrentPolicyVersion,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = userAgent ?? string.Empty,
            AcceptedOnUtc = DateTime.UtcNow
        };

        record.ContentHash = ComputeHash(record);

        await _repository.InsertAsync(record);
        return record;
    }

    /// <inheritdoc />
    public virtual async Task<CookieConsent?> GetLatestAsync(Guid consentGuid)
    {
        if (consentGuid == Guid.Empty)
            return null;

        var list = await _repository.GetAllAsync(
            query => query.Where(c => c.ConsentGuid == consentGuid)
                          .OrderByDescending(c => c.AcceptedOnUtc),
            getCacheKey: null);
        return list.FirstOrDefault();
    }

    /// <inheritdoc />
    public virtual async Task<CookieConsent?> GetLatestForCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            return null;

        var list = await _repository.GetAllAsync(
            query => query.Where(c => c.CustomerId == customerId)
                          .OrderByDescending(c => c.AcceptedOnUtc),
            getCacheKey: null);
        return list.FirstOrDefault();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// SHA-256 hash — kategori bayrakları + politika versiyonu + timestamp + GUID.
    /// Kayıtta manipülasyon olursa hash uyuşmaz.
    /// Public static — denetim araçları ve testler için doğrudan kullanılabilir.
    /// </summary>
    public static string ComputeHash(CookieConsent record)
    {
        var payload = $"{record.ConsentGuid:N}|{record.CustomerId}|" +
                      $"{record.FunctionalAllowed}|{record.AnalyticsAllowed}|{record.MarketingAllowed}|" +
                      $"{record.PolicyVersion}|{record.AcceptedOnUtc:O}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    #endregion
}
