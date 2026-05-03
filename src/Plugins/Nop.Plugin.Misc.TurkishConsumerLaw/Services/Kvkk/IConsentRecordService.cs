using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;

/// <summary>
/// KVKK / ETK rıza kayıtları audit servisi.
///
/// Append-only: her kayıt değişikliği yeni satır oluşturur. UPDATE yok.
/// "Mevcut etkin rıza" = scope başına en güncel satır.
/// </summary>
public interface IConsentRecordService
{
    /// <summary>
    /// Tek bir scope için onay/red kaydı ekler. SHA-256 hash + IP/UA otomatik.
    /// </summary>
    Task<ConsentRecord> RecordAsync(
        int customerId,
        ConsentScope scope,
        bool granted,
        string textVersion,
        ConsentSource source,
        string? ipAddress,
        string? userAgent);

    /// <summary>
    /// Birden fazla scope için tek seferde kayıt — kayıt formundan gelir.
    /// </summary>
    Task<IList<ConsentRecord>> RecordBatchAsync(
        int customerId,
        IEnumerable<(ConsentScope Scope, bool Granted, string TextVersion)> consents,
        ConsentSource source,
        string? ipAddress,
        string? userAgent);

    /// <summary>
    /// Müşterinin scope başına en güncel rıza durumunu döner (effective state).
    /// </summary>
    Task<IDictionary<ConsentScope, ConsentRecord>> GetEffectiveAsync(int customerId);

    /// <summary>
    /// Müşterinin tüm rıza tarihçesini (audit trail) döner.
    /// </summary>
    Task<IList<ConsentRecord>> GetHistoryAsync(int customerId);
}
