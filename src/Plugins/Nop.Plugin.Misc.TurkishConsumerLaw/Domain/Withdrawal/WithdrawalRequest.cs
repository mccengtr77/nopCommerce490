using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;

/// <summary>
/// Cayma hakkı (geri iade) talebi.
///
/// 6502 sayılı Tüketici Kanunu m.48 ve Mesafeli Sözleşmeler Yönetmeliği m.9
/// kapsamında 14 gün içinde gerekçesiz iade hakkı.
/// </summary>
public class WithdrawalRequest : BaseEntity
{
    /// <summary>nopCommerce sipariş Id</summary>
    public int OrderId { get; set; }

    /// <summary>Müşteri Id (kayıtlı müşteri kayıt zorunlu — sipariş kayıtla yapılır)</summary>
    public int CustomerId { get; set; }

    /// <summary>Cayma nedeni (opsiyonel, analiz için)</summary>
    public WithdrawalReason Reason { get; set; } = WithdrawalReason.NotSpecified;

    /// <summary>Müşteri açıklaması</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Talep durumu. Admin state machine'i ilerletir.
    /// </summary>
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

    /// <summary>
    /// Reddedildiyse gerekçe (admin yazdı)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Onay verildi mi (state machine'in eşiği — bir kere true olunca refund/inspect akışı başlar)
    /// </summary>
    public DateTime? ApprovedOnUtc { get; set; }

    /// <summary>
    /// İade kargo takip numarası (kargo plugin'i ile entegrasyon Faz 2'de)
    /// </summary>
    public string? ReturnTrackingNumber { get; set; }

    /// <summary>
    /// Ürün firmaya ulaştığında set edilir
    /// </summary>
    public DateTime? ReceivedOnUtc { get; set; }

    /// <summary>
    /// Hasar/inceleme notu (ürün açılmış mı, aksesuarları eksik mi vb.)
    /// </summary>
    public string? InspectionNotes { get; set; }

    /// <summary>
    /// İade edilen tutar (TRY) — kısmi iade için ayrı satır
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Refund yapıldı mı timestamp
    /// </summary>
    public DateTime? RefundedOnUtc { get; set; }

    /// <summary>
    /// Süreç tamamen kapandı mı
    /// </summary>
    public DateTime? CompletedOnUtc { get; set; }

    public DateTime SubmittedOnUtc { get; set; }

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Mağaza Id
    /// </summary>
    public int StoreId { get; set; }
}
