namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;

/// <summary>
/// Bir siparişin cayma hakkına uygun olup olmadığını değerlendirir.
///
/// 6502 m.48 ve Mesafeli Sözleşmeler Yönetmeliği m.9: 14 gün cayma süresi.
/// Sayaç başlangıcı = ürün teslimat tarihi (varsa) ya da sipariş tarihi.
///
/// Yönetmelik m.15 istisnaları (kişiselleştirilmiş ürün, hijyenik ambalaj açılmış vb.)
/// product/category bazlı tanımlanabilir — bu sürüm temel kontroller (deadline + payment status).
/// İstisna kuralları sonraki sürümde <c>WithdrawalExclusionRule</c> entity ile genişletilir.
/// </summary>
public interface IWithdrawalEligibilityChecker
{
    /// <summary>
    /// Siparişin cayma hakkına uygun olup olmadığını kontrol eder.
    /// </summary>
    Task<WithdrawalEligibilityResult> CheckAsync(int orderId);
}

/// <summary>
/// Eligibility kontrolünün sonucu.
/// </summary>
/// <param name="IsEligible">Cayma hakkı kullanılabilir mi</param>
/// <param name="Reason">Uygun değilse açıklama (UI'da gösterilir)</param>
/// <param name="OrderId">Sipariş Id</param>
/// <param name="DeadlineUtc">Cayma yapılabilecek son tarih (eligible ise)</param>
/// <param name="DaysRemaining">Deadline'a kalan gün (negatif = süre dolmuş)</param>
public record WithdrawalEligibilityResult(
    bool IsEligible,
    string? Reason,
    int OrderId,
    DateTime? DeadlineUtc,
    int? DaysRemaining);
