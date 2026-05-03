using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;

/// <summary>
/// Cayma talebi yönetim servisi (CRUD + state machine).
///
/// State machine geçişleri (admin tarafından yönetilir):
///   Pending → Approved | Rejected | Cancelled
///   Approved → InTransit
///   InTransit → Received
///   Received → Inspected
///   Inspected → Refunded
///   Refunded → Completed
/// </summary>
public interface IWithdrawalService
{
    /// <summary>
    /// Müşteri tarafından yeni cayma talebi oluştur (eligibility kontrolünden sonra).
    /// SubmittedOnUtc, Status=Pending, hash otomatik set edilir.
    /// </summary>
    Task<WithdrawalRequest> SubmitAsync(WithdrawalRequest request);

    Task<WithdrawalRequest?> GetByIdAsync(int id);

    /// <summary>
    /// Belirli sipariş için en güncel cayma talebi (sipariş başına 1 aktif olur).
    /// </summary>
    Task<WithdrawalRequest?> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Müşterinin cayma talepleri (kendi panelinde göster).
    /// </summary>
    Task<IList<WithdrawalRequest>> GetByCustomerAsync(int customerId);

    /// <summary>
    /// Admin için durum filtreli liste.
    /// </summary>
    Task<IList<WithdrawalRequest>> GetByStatusAsync(WithdrawalStatus? status = null);

    /// <summary>
    /// Admin onaylar (Pending → Approved).
    /// </summary>
    Task ApproveAsync(int id);

    /// <summary>
    /// Admin reddeder (Pending → Rejected, gerekçe ile).
    /// </summary>
    Task RejectAsync(int id, string rejectionReason);

    /// <summary>
    /// Müşteri talebi geri çekti (Pending → Cancelled).
    /// </summary>
    Task CancelByCustomerAsync(int id, int customerId);

    /// <summary>
    /// State machine'i bir adım ilerlet. Admin "İade kargosu yola çıktı" / "Ürün ulaştı" /
    /// "İncelendi" / "Refund yapıldı" gibi state'leri set eder.
    /// </summary>
    Task AdvanceAsync(int id, WithdrawalStatus newStatus, string? note = null, decimal? refundAmount = null);
}
