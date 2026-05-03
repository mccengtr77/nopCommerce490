using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.Withdrawal;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class WithdrawalController : BasePluginController
{
    protected readonly IWithdrawalService _service;
    protected readonly IPermissionService _permissionService;
    protected readonly INotificationService _notificationService;
    protected readonly ILocalizationService _localizationService;

    public WithdrawalController(
        IWithdrawalService service,
        IPermissionService permissionService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _service = service;
        _permissionService = permissionService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    public async Task<IActionResult> List(WithdrawalStatus? status = null)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var requests = await _service.GetByStatusAsync(status);

        var model = new WithdrawalListModel
        {
            FilterStatus = status,
            Items = requests.Select(r => new WithdrawalRowModel
            {
                Id = r.Id,
                OrderId = r.OrderId,
                CustomerId = r.CustomerId,
                Status = r.Status,
                StatusLabel = GetStatusLabel(r.Status),
                ReasonLabel = GetReasonLabel(r.Reason),
                SubmittedOnUtc = r.SubmittedOnUtc,
                RefundedOnUtc = r.RefundedOnUtc,
                RefundAmount = r.RefundAmount
            }).ToList()
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Withdrawal/List.cshtml", model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var request = await _service.GetByIdAsync(id);
        if (request is null)
            return RedirectToAction("List");

        var model = new WithdrawalDetailModel
        {
            Id = request.Id,
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Status = request.Status,
            StatusLabel = GetStatusLabel(request.Status),
            Reason = request.Reason,
            ReasonLabel = GetReasonLabel(request.Reason),
            Description = request.Description,
            SubmittedOnUtc = request.SubmittedOnUtc,
            ApprovedOnUtc = request.ApprovedOnUtc,
            ReceivedOnUtc = request.ReceivedOnUtc,
            RefundedOnUtc = request.RefundedOnUtc,
            CompletedOnUtc = request.CompletedOnUtc,
            RejectionReason = request.RejectionReason,
            ReturnTrackingNumber = request.ReturnTrackingNumber,
            InspectionNotes = request.InspectionNotes,
            RefundAmount = request.RefundAmount,
            IpAddress = request.IpAddress,
            AvailableNextStates = GetAvailableTransitions(request.Status)
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Withdrawal/Edit.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        try
        {
            await _service.ApproveAsync(id);
            _notificationService.SuccessNotification("Talep onaylandı.");
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(ex.Message);
        }

        return RedirectToAction("Edit", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(WithdrawalRejectModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        try
        {
            await _service.RejectAsync(model.Id, model.RejectionReason);
            _notificationService.SuccessNotification("Talep reddedildi.");
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(ex.Message);
        }

        return RedirectToAction("Edit", new { id = model.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Advance(WithdrawalAdvanceModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        try
        {
            await _service.AdvanceAsync(model.Id, model.NewStatus, model.Note, model.RefundAmount);
            _notificationService.SuccessNotification($"Durum güncellendi: {GetStatusLabel(model.NewStatus)}");
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(ex.Message);
        }

        return RedirectToAction("Edit", new { id = model.Id });
    }

    private static IList<WithdrawalStatus> GetAvailableTransitions(WithdrawalStatus current)
    {
        var all = Enum.GetValues<WithdrawalStatus>();
        return all.Where(s => WithdrawalService.IsValidTransition(current, s)).ToList();
    }

    private static string GetStatusLabel(WithdrawalStatus status) => status switch
    {
        WithdrawalStatus.Pending    => "Bekliyor",
        WithdrawalStatus.Approved   => "Onaylandı",
        WithdrawalStatus.Rejected   => "Reddedildi",
        WithdrawalStatus.InTransit  => "Kargoda",
        WithdrawalStatus.Received   => "Ulaştı",
        WithdrawalStatus.Inspected  => "İncelendi",
        WithdrawalStatus.Refunded   => "İade Yapıldı",
        WithdrawalStatus.Completed  => "Tamamlandı",
        WithdrawalStatus.Cancelled  => "İptal",
        _                           => status.ToString()
    };

    private static string GetReasonLabel(WithdrawalReason r) => r switch
    {
        WithdrawalReason.NotSpecified      => "—",
        WithdrawalReason.ChangedMind       => "Fikir değişikliği",
        WithdrawalReason.QualityIssue      => "Kalite sorunu",
        WithdrawalReason.Defective         => "Arızalı/hasarlı",
        WithdrawalReason.WrongProduct      => "Yanlış ürün",
        WithdrawalReason.LateDelivery      => "Geç teslim",
        WithdrawalReason.BetterAlternative => "Daha iyi alternatif",
        WithdrawalReason.Other             => "Diğer",
        _                                  => r.ToString()
    };
}
