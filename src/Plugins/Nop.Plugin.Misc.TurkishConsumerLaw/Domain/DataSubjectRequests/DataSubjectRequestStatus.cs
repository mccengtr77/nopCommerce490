namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;

/// <summary>
/// KVKK m.11 başvuru durumu.
/// </summary>
public enum DataSubjectRequestStatus
{
    /// <summary>Başvuru yeni alındı, işleme alınmadı</summary>
    Submitted = 1,

    /// <summary>Admin başvuruyu incelemekte</summary>
    InReview = 2,

    /// <summary>Ek bilgi/belge istendi (sayaç durmaz, KVKK m.13)</summary>
    AdditionalInfoRequested = 3,

    /// <summary>Başvuru kabul edildi ve işlem yapıldı</summary>
    Approved = 4,

    /// <summary>Başvuru reddedildi (gerekçe açıklanmalı)</summary>
    Rejected = 5,

    /// <summary>Başvuru sahibi geri çekti</summary>
    Withdrawn = 6
}
