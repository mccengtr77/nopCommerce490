using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.Withdrawal;

public record WithdrawalListModel : BaseNopModel
{
    public WithdrawalStatus? FilterStatus { get; set; }
    public IList<WithdrawalRowModel> Items { get; set; } = new List<WithdrawalRowModel>();
}

public record WithdrawalRowModel : BaseNopEntityModel
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public WithdrawalStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string ReasonLabel { get; set; } = string.Empty;
    public DateTime SubmittedOnUtc { get; set; }
    public DateTime? RefundedOnUtc { get; set; }
    public decimal? RefundAmount { get; set; }
}

public record WithdrawalDetailModel : BaseNopEntityModel
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public WithdrawalStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public WithdrawalReason Reason { get; set; }
    public string ReasonLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime SubmittedOnUtc { get; set; }
    public DateTime? ApprovedOnUtc { get; set; }
    public DateTime? ReceivedOnUtc { get; set; }
    public DateTime? RefundedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public string? RejectionReason { get; set; }
    public string? ReturnTrackingNumber { get; set; }
    public string? InspectionNotes { get; set; }
    public decimal? RefundAmount { get; set; }
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Şu an mümkün olan state geçişleri (admin butonları için).
    /// </summary>
    public IList<WithdrawalStatus> AvailableNextStates { get; set; } = new List<WithdrawalStatus>();
}

public record WithdrawalAdvanceModel
{
    public int Id { get; set; }
    public WithdrawalStatus NewStatus { get; set; }
    public string? Note { get; set; }
    public decimal? RefundAmount { get; set; }
}

public record WithdrawalRejectModel
{
    public int Id { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Withdrawal.RejectionReason")]
    public string RejectionReason { get; set; } = string.Empty;
}
