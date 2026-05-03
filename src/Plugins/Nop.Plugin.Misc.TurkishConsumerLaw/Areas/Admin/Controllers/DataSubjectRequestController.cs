using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.DataSubjectRequests;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.DataSubjectRequests;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Controllers;

/// <summary>
/// KVKK m.11 başvurularını admin'in yönetebilmesi için liste + yanıt akışı.
/// </summary>
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class DataSubjectRequestController : BasePluginController
{
    protected readonly IDataSubjectRequestService _service;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;

    public DataSubjectRequestController(
        IDataSubjectRequestService service,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _service = service;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> List(DataSubjectRequestStatus? status = null)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var requests = await _service.GetByStatusAsync(status);
        var overdue = await _service.GetOverdueAsync();
        var now = DateTime.UtcNow;

        var model = new DataSubjectRequestListModel
        {
            FilterStatus = status,
            OverdueCount = overdue.Count,
            Items = requests.Select(r => new DataSubjectRequestRowModel
            {
                Id = r.Id,
                FullName = r.FullName,
                Email = r.Email,
                RequestTypeLabel = GetTypeLabel(r.RequestType),
                StatusLabel = GetStatusLabel(r.Status),
                Status = r.Status,
                SubmittedOnUtc = r.SubmittedOnUtc,
                DeadlineUtc = r.DeadlineUtc,
                DaysToDeadline = (r.DeadlineUtc - now).Days,
                RespondedOnUtc = r.RespondedOnUtc
            }).ToList()
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/DataSubjectRequests/List.cshtml", model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var request = await _service.GetByIdAsync(id);
        if (request is null)
            return RedirectToAction("List");

        var model = new DataSubjectRequestDetailModel
        {
            Id = request.Id,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            CustomerId = request.CustomerId,
            RequestType = request.RequestType,
            RequestTypeLabel = GetTypeLabel(request.RequestType),
            Description = request.Description,
            SubmittedOnUtc = request.SubmittedOnUtc,
            DeadlineUtc = request.DeadlineUtc,
            DaysToDeadline = (request.DeadlineUtc - DateTime.UtcNow).Days,
            RespondedOnUtc = request.RespondedOnUtc,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            Status = request.Status,
            AdminResponse = request.AdminResponse
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/DataSubjectRequests/Edit.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(DataSubjectRequestDetailModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        try
        {
            await _service.RespondAsync(model.Id, model.Status, model.AdminResponse);
            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Common.DataEditedSuccessfully"));
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification($"Yanıt kaydedilemedi: {ex.Message}");
        }

        return RedirectToAction("Edit", new { id = model.Id });
    }

    private static string GetTypeLabel(DataSubjectRequestType type) => type switch
    {
        DataSubjectRequestType.Inquire                 => "İşlenme bilgisi (m.11/a)",
        DataSubjectRequestType.Access                  => "Bilgi talep (m.11/b)",
        DataSubjectRequestType.PurposeInfo             => "Amaç bilgisi (m.11/c)",
        DataSubjectRequestType.Rectification           => "Düzeltme (m.11/d)",
        DataSubjectRequestType.Erasure                 => "Silme (m.11/e)",
        DataSubjectRequestType.NotifyThirdParties      => "3. taraf bildirimi (m.11/f)",
        DataSubjectRequestType.ObjectAutomatedDecision => "Otomatik karar itirazı (m.11/g)",
        DataSubjectRequestType.Compensation            => "Tazminat (m.11/h)",
        DataSubjectRequestType.Portability             => "Veri taşınabilirliği",
        _                                              => type.ToString()
    };

    private static string GetStatusLabel(DataSubjectRequestStatus status) => status switch
    {
        DataSubjectRequestStatus.Submitted              => "Yeni",
        DataSubjectRequestStatus.InReview               => "İncelemede",
        DataSubjectRequestStatus.AdditionalInfoRequested => "Ek bilgi istendi",
        DataSubjectRequestStatus.Approved               => "Kabul edildi",
        DataSubjectRequestStatus.Rejected               => "Reddedildi",
        DataSubjectRequestStatus.Withdrawn              => "Geri çekildi",
        _                                               => status.ToString()
    };
}
