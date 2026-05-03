using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.DataSubjectRequests;

/// <summary>
/// KVKK m.11 veri sahibi başvuru servisi.
/// </summary>
public interface IDataSubjectRequestService
{
    /// <summary>
    /// Yeni başvuru oluşturur. SubmittedOnUtc + DeadlineUtc (30 gün) otomatik set edilir.
    /// Başvuru e-postasının doğrulanması farklı bir akıştır (e-posta gönderimi → token).
    /// </summary>
    Task<DataSubjectRequest> SubmitAsync(DataSubjectRequest request);

    /// <summary>
    /// Id ile başvuruyu getirir.
    /// </summary>
    Task<DataSubjectRequest?> GetByIdAsync(int id);

    /// <summary>
    /// Müşteri başvurularını döner (kayıtlı müşteri kendi başvurularını görmek için).
    /// </summary>
    Task<IList<DataSubjectRequest>> GetByCustomerAsync(int customerId);

    /// <summary>
    /// Admin için: belirli durumda olan başvuruları (yaklaşan deadline'lar dahil) döner.
    /// </summary>
    Task<IList<DataSubjectRequest>> GetByStatusAsync(DataSubjectRequestStatus? status = null);

    /// <summary>
    /// Admin yanıt yazıp durumu günceller. <see cref="DataSubjectRequest.RespondedOnUtc"/> set edilir.
    /// Status Approved/Rejected ise sayaç durur.
    /// </summary>
    Task RespondAsync(int requestId, DataSubjectRequestStatus newStatus, string? adminResponse);

    /// <summary>
    /// Yasal süreyi geçmiş (deadline aşımı) açık başvuruları döner — admin uyarı paneli için.
    /// </summary>
    Task<IList<DataSubjectRequest>> GetOverdueAsync();
}
