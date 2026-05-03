namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;

/// <summary>
/// KVKK m.11 ile veri sahibinin kullanabileceği haklar.
///
/// Veri sorumlusu, başvuruyu en geç 30 gün içinde sonuçlandırmak zorundadır
/// (KVKK m.13/(2)). Süre sayacı <see cref="DataSubjectRequest.SubmittedOnUtc"/>'den başlar.
/// </summary>
public enum DataSubjectRequestType
{
    /// <summary>
    /// İşlenip işlenmediğini öğrenme — KVKK m.11/(a)
    /// </summary>
    Inquire = 1,

    /// <summary>
    /// İşlenmişse buna ilişkin bilgi talep etme — KVKK m.11/(b)
    /// </summary>
    Access = 2,

    /// <summary>
    /// İşlenme amacı ve uygun kullanılıp kullanılmadığını öğrenme — KVKK m.11/(c)
    /// </summary>
    PurposeInfo = 3,

    /// <summary>
    /// Eksik/yanlış işlenmişse düzeltilmesini isteme — KVKK m.11/(d)
    /// </summary>
    Rectification = 4,

    /// <summary>
    /// Silinmesini veya yok edilmesini isteme — KVKK m.11/(e)
    /// </summary>
    Erasure = 5,

    /// <summary>
    /// Aktarıldığı 3. kişilere bildirme yükümlülüğü — KVKK m.11/(f)
    /// </summary>
    NotifyThirdParties = 6,

    /// <summary>
    /// Otomatik sistem analizine itiraz — KVKK m.11/(g)
    /// </summary>
    ObjectAutomatedDecision = 7,

    /// <summary>
    /// Kanuna aykırı işleme nedeniyle uğranılan zararın tazmini — KVKK m.11/(h)
    /// </summary>
    Compensation = 8,

    /// <summary>
    /// Veri taşınabilirliği (KVKK kapsamında doğrudan yer almasa da uygulamada talep edilebilir)
    /// </summary>
    Portability = 9
}
