using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.DataSubjectRequests;

/// <summary>
/// DSR liste sayfası için model — basit liste (DataTables sonradan eklenir).
/// </summary>
public record DataSubjectRequestListModel : BaseNopModel
{
    public IList<DataSubjectRequestRowModel> Items { get; set; } = new List<DataSubjectRequestRowModel>();

    /// <summary>
    /// Yasal süre aşımı yapan başvuru sayısı — uyarı banner'ı için
    /// </summary>
    public int OverdueCount { get; set; }

    /// <summary>
    /// Filtrelenecek durum (null = hepsi)
    /// </summary>
    public DataSubjectRequestStatus? FilterStatus { get; set; }
}

public record DataSubjectRequestRowModel : BaseNopEntityModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RequestTypeLabel { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DataSubjectRequestStatus Status { get; set; }
    public DateTime SubmittedOnUtc { get; set; }
    public DateTime DeadlineUtc { get; set; }

    /// <summary>
    /// Deadline'a kalan gün — negatif ise süre aşımı
    /// </summary>
    public int DaysToDeadline { get; set; }

    /// <summary>
    /// Yanıt verildiyse responseBy zaman, hala açık ise null
    /// </summary>
    public DateTime? RespondedOnUtc { get; set; }
}

/// <summary>
/// DSR detay/yanıt sayfası modeli.
/// </summary>
public record DataSubjectRequestDetailModel : BaseNopEntityModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int CustomerId { get; set; }
    public DataSubjectRequestType RequestType { get; set; }
    public string RequestTypeLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime SubmittedOnUtc { get; set; }
    public DateTime DeadlineUtc { get; set; }
    public int DaysToDeadline { get; set; }
    public DateTime? RespondedOnUtc { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Dsr.Status")]
    public DataSubjectRequestStatus Status { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Dsr.AdminResponse")]
    public string? AdminResponse { get; set; }
}
