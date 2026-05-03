using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;

/// <summary>
/// KVKK m.11 veri sahibi başvurusu.
///
/// Veri sorumlusu en geç 30 gün içinde başvuruyu sonuçlandırır (KVKK m.13/(2)).
/// Başvuru sahibinin kayıtlı müşteri olması zorunlu değildir; e-posta üzerinden
/// kimlik teyidi yapılır (Aydınlatma Yükümlülüğü Tebliği m.5).
/// </summary>
public class DataSubjectRequest : BaseEntity
{
    /// <summary>
    /// Müşteri Id (anonim başvuru ise 0)
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Başvuru sahibi adı (anonim başvuru için form'dan alınır)
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// İletişim e-postası (yanıt buraya gider)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// İletişim telefonu (opsiyonel)
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Başvuru türü — KVKK m.11 hakları
    /// </summary>
    public DataSubjectRequestType RequestType { get; set; }

    /// <summary>
    /// Talep detayı / açıklama
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Mevcut durum
    /// </summary>
    public DataSubjectRequestStatus Status { get; set; } = DataSubjectRequestStatus.Submitted;

    /// <summary>
    /// Admin yanıtı / işlem notu
    /// </summary>
    public string? AdminResponse { get; set; }

    /// <summary>
    /// Yasal sayaç başlangıcı — KVKK m.13/(2) 30 gün buradan sayılır
    /// </summary>
    public DateTime SubmittedOnUtc { get; set; }

    /// <summary>
    /// Yanıt tamamlanma zamanı (Approved/Rejected'de set edilir)
    /// </summary>
    public DateTime? RespondedOnUtc { get; set; }

    /// <summary>
    /// Yasal süre limiti (SubmittedOnUtc + 30 gün) — admin UI'da
    /// kalan gün sayacı için kullanılır.
    /// </summary>
    public DateTime DeadlineUtc { get; set; }

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Mağaza Id (multi-store ortamlar için)
    /// </summary>
    public int StoreId { get; set; }
}
