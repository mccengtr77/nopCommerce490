namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;

/// <summary>
/// Cayma talebi state machine.
///
/// Akış: Pending → Approved → InTransit → Received → Inspected → Refunded → Completed
/// İstisnalar: Pending → Rejected, Pending → Cancelled (müşteri iptal)
///
/// 2026 Mesafeli Sözleşmeler Yönetmeliği değişiklikleri:
/// - İade kargo ücretini SATICI öder (m.13/(2)) → InTransit aşamasında müşteriden ücret alınmaz
/// - 14 gün cayma süresi (m.9)
/// </summary>
public enum WithdrawalStatus
{
    /// <summary>Müşteri talep oluşturdu, admin onayını bekliyor</summary>
    Pending = 1,

    /// <summary>Admin talebi onayladı, iade kargosu hazırlanıyor</summary>
    Approved = 2,

    /// <summary>Talep reddedildi (gerekçe açıklanmalı)</summary>
    Rejected = 3,

    /// <summary>İade kargosu yola çıktı (tracking no var)</summary>
    InTransit = 4,

    /// <summary>İade ürün firmaya ulaştı, hasar/inceleme bekliyor</summary>
    Received = 5,

    /// <summary>Ürün incelendi, refund'a hazır</summary>
    Inspected = 6,

    /// <summary>Para iadesi yapıldı (ödeme provider'ından refund)</summary>
    Refunded = 7,

    /// <summary>Süreç tamamlandı (tüm taraflar bilgilendirildi)</summary>
    Completed = 8,

    /// <summary>Müşteri talebi geri çekti</summary>
    Cancelled = 9
}
