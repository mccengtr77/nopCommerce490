using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Components.WithdrawalRequestForm;

/// <summary>
/// Müşteri panelindeki sipariş detayında "Cayma Talebi Oluştur" formu.
///
/// Kullanım:
///   @await Component.InvokeAsync("WithdrawalRequestForm", new { orderId = Model.Id })
///
/// Form server-side eligibility kontrolü ile yüklenir, JS ile AJAX submit edilir.
/// </summary>
public class WithdrawalRequestFormViewComponent : NopViewComponent
{
    protected readonly IWithdrawalEligibilityChecker _eligibilityChecker;
    protected readonly IWithdrawalService _service;

    public WithdrawalRequestFormViewComponent(
        IWithdrawalEligibilityChecker eligibilityChecker,
        IWithdrawalService service)
    {
        _eligibilityChecker = eligibilityChecker;
        _service = service;
    }

    public async Task<IViewComponentResult> InvokeAsync(int orderId)
    {
        var eligibility = await _eligibilityChecker.CheckAsync(orderId);
        var existing = await _service.GetByOrderIdAsync(orderId);

        var hasActiveRequest = existing is not null
            && existing.Status is not (WithdrawalStatus.Rejected or WithdrawalStatus.Cancelled);

        var reasons = Enum.GetValues<WithdrawalReason>()
            .Select(r => new ReasonOption((int)r, GetReasonLabel(r)))
            .ToList();

        var model = new WithdrawalRequestFormModel
        {
            OrderId = orderId,
            IsEligible = eligibility.IsEligible && !hasActiveRequest,
            Reason = hasActiveRequest
                ? $"Bu sipariş için aktif cayma talebi var (#{existing!.Id}, durum: {existing.Status})."
                : eligibility.Reason,
            DeadlineUtc = eligibility.DeadlineUtc,
            DaysRemaining = eligibility.DaysRemaining,
            ExistingRequestId = existing?.Id,
            ExistingStatus = existing?.Status,
            Reasons = reasons
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Components/WithdrawalRequestForm/Default.cshtml", model);
    }

    private static string GetReasonLabel(WithdrawalReason r) => r switch
    {
        WithdrawalReason.NotSpecified      => "Belirtmek istemiyorum",
        WithdrawalReason.ChangedMind       => "Fikrimi değiştirdim",
        WithdrawalReason.QualityIssue      => "Ürün kalitesi yetersiz",
        WithdrawalReason.Defective         => "Ürün arızalı/hasarlı",
        WithdrawalReason.WrongProduct      => "Yanlış ürün gönderildi",
        WithdrawalReason.LateDelivery      => "Çok geç teslim edildi",
        WithdrawalReason.BetterAlternative => "Daha iyi alternatif buldum",
        WithdrawalReason.Other             => "Diğer",
        _                                  => r.ToString()
    };
}

public class WithdrawalRequestFormModel
{
    public int OrderId { get; set; }
    public bool IsEligible { get; set; }
    public string? Reason { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public int? DaysRemaining { get; set; }
    public int? ExistingRequestId { get; set; }
    public WithdrawalStatus? ExistingStatus { get; set; }
    public IList<ReasonOption> Reasons { get; set; } = new List<ReasonOption>();
}

public record ReasonOption(int Value, string Label);
