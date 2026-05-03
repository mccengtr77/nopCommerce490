using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.Contracts;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;
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
public class ContractsController : BasePluginController
{
    protected readonly IContractTemplateService _templateService;
    protected readonly IContractGenerationService _generationService;
    protected readonly IPermissionService _permissionService;
    protected readonly INotificationService _notificationService;
    protected readonly ILocalizationService _localizationService;

    public ContractsController(
        IContractTemplateService templateService,
        IContractGenerationService generationService,
        IPermissionService permissionService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _templateService = templateService;
        _generationService = generationService;
        _permissionService = permissionService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    #region Template CRUD

    public async Task<IActionResult> Templates()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var templates = await _templateService.GetAllAsync();
        var model = new ContractTemplateListModel
        {
            Items = templates.Select(t => new ContractTemplateRowModel
            {
                Id = t.Id,
                Type = t.Type,
                TypeLabel = TypeLabel(t.Type),
                Name = t.Name,
                Version = t.Version,
                IsActive = t.IsActive,
                DisplayOrder = t.DisplayOrder,
                UpdatedOnUtc = t.UpdatedOnUtc
            }).ToList()
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Contracts/Templates.cshtml", model);
    }

    public async Task<IActionResult> CreateTemplate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Contracts/EditTemplate.cshtml",
            new ContractTemplateEditModel { Version = "1.0", IsActive = true });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTemplate(ContractTemplateEditModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.HtmlContent))
            return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Contracts/EditTemplate.cshtml", model);

        await _templateService.InsertAsync(new ContractTemplate
        {
            Type = model.Type,
            Name = model.Name.Trim(),
            HtmlContent = model.HtmlContent,
            Version = model.Version,
            IsActive = model.IsActive,
            DisplayOrder = model.DisplayOrder
        });

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Common.DataAddedSuccessfully"));

        return RedirectToAction("Templates");
    }

    public async Task<IActionResult> EditTemplate(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var template = await _templateService.GetByIdAsync(id);
        if (template is null) return RedirectToAction("Templates");

        var model = new ContractTemplateEditModel
        {
            Id = template.Id,
            Type = template.Type,
            Name = template.Name,
            HtmlContent = template.HtmlContent,
            Version = template.Version,
            IsActive = template.IsActive,
            DisplayOrder = template.DisplayOrder
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Contracts/EditTemplate.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditTemplate(ContractTemplateEditModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var template = await _templateService.GetByIdAsync(model.Id);
        if (template is null) return RedirectToAction("Templates");

        template.Type = model.Type;
        template.Name = model.Name.Trim();
        template.HtmlContent = model.HtmlContent;
        template.Version = model.Version;
        template.IsActive = model.IsActive;
        template.DisplayOrder = model.DisplayOrder;

        await _templateService.UpdateAsync(template);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Common.DataEditedSuccessfully"));

        return RedirectToAction("Templates");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var template = await _templateService.GetByIdAsync(id);
        if (template is not null)
            await _templateService.DeleteAsync(template);

        return RedirectToAction("Templates");
    }

    #endregion

    #region Generated contracts

    public async Task<IActionResult> Generated(int? orderId, int? customerId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        IList<GeneratedContract> contracts;
        if (orderId is > 0)
            contracts = await _generationService.GetByOrderIdAsync(orderId.Value);
        else if (customerId is > 0)
            contracts = await _generationService.GetByCustomerAsync(customerId.Value);
        else
            contracts = new List<GeneratedContract>();

        var model = new GeneratedContractListModel
        {
            Items = contracts.Select(c => new GeneratedContractRowModel
            {
                Id = c.Id,
                OrderId = c.OrderId,
                CustomerId = c.CustomerId,
                Type = c.Type,
                TypeLabel = TypeLabel(c.Type),
                TemplateVersion = c.TemplateVersion,
                Accepted = c.Accepted,
                CreatedOnUtc = c.CreatedOnUtc,
                AcceptedOnUtc = c.AcceptedOnUtc
            }).ToList()
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Contracts/Generated.cshtml", model);
    }

    public async Task<IActionResult> ViewGenerated(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var contract = await _generationService.GetByIdAsync(id);
        if (contract is null) return RedirectToAction("Generated");

        // Inline HTML görüntüleme
        return Content(contract.HtmlContent, "text/html; charset=utf-8");
    }

    #endregion

    private static string TypeLabel(ContractTemplateType t) => t switch
    {
        ContractTemplateType.Mss => "MSS — Mesafeli Satış Sözleşmesi",
        ContractTemplateType.Obf => "ÖBF — Ön Bilgilendirme Formu",
        _                        => t.ToString()
    };
}
