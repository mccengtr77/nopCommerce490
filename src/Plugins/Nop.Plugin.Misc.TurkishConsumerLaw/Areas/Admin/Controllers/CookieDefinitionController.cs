using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.Cookies;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;
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
public class CookieDefinitionController : BasePluginController
{
    protected readonly ICookieDefinitionService _service;
    protected readonly IPermissionService _permissionService;
    protected readonly INotificationService _notificationService;
    protected readonly ILocalizationService _localizationService;

    public CookieDefinitionController(
        ICookieDefinitionService service,
        IPermissionService permissionService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _service = service;
        _permissionService = permissionService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var cookies = await _service.GetActiveAsync();

        var model = new CookieDefinitionListModel
        {
            Items = cookies.Select(c => new CookieDefinitionRowModel
            {
                Id = c.Id,
                Name = c.Name,
                Provider = c.Provider,
                Purpose = c.Purpose,
                Duration = c.Duration,
                Category = c.Category,
                CategoryLabel = GetCategoryLabel(c.Category),
                IsThirdParty = c.IsThirdParty,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder
            }).ToList()
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Cookies/List.cshtml", model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Cookies/Edit.cshtml",
            new CookieDefinitionEditModel { Category = CookieCategory.Functional, IsActive = true, DisplayOrder = 100 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CookieDefinitionEditModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Name))
            return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Cookies/Edit.cshtml", model);

        await _service.InsertAsync(new CookieDefinition
        {
            Name = model.Name.Trim(),
            Provider = model.Provider?.Trim() ?? string.Empty,
            Purpose = model.Purpose?.Trim() ?? string.Empty,
            Duration = model.Duration?.Trim() ?? string.Empty,
            Category = model.Category,
            IsThirdParty = model.IsThirdParty,
            IsActive = model.IsActive,
            DisplayOrder = model.DisplayOrder
        });

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Common.DataAddedSuccessfully"));

        return RedirectToAction("List");
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var cookie = await _service.GetByIdAsync(id);
        if (cookie is null)
            return RedirectToAction("List");

        var model = new CookieDefinitionEditModel
        {
            Id = cookie.Id,
            Name = cookie.Name,
            Provider = cookie.Provider,
            Purpose = cookie.Purpose,
            Duration = cookie.Duration,
            Category = cookie.Category,
            IsThirdParty = cookie.IsThirdParty,
            IsActive = cookie.IsActive,
            DisplayOrder = cookie.DisplayOrder
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Cookies/Edit.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CookieDefinitionEditModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var cookie = await _service.GetByIdAsync(model.Id);
        if (cookie is null)
            return RedirectToAction("List");

        cookie.Name = model.Name.Trim();
        cookie.Provider = model.Provider?.Trim() ?? string.Empty;
        cookie.Purpose = model.Purpose?.Trim() ?? string.Empty;
        cookie.Duration = model.Duration?.Trim() ?? string.Empty;
        cookie.Category = model.Category;
        cookie.IsThirdParty = model.IsThirdParty;
        cookie.IsActive = model.IsActive;
        cookie.DisplayOrder = model.DisplayOrder;

        await _service.UpdateAsync(cookie);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Common.DataEditedSuccessfully"));

        return RedirectToAction("List");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var cookie = await _service.GetByIdAsync(id);
        if (cookie is not null)
            await _service.DeleteAsync(cookie);

        return RedirectToAction("List");
    }

    private static string GetCategoryLabel(CookieCategory cat) => cat switch
    {
        CookieCategory.Necessary  => "Zorunlu",
        CookieCategory.Functional => "İşlevsellik",
        CookieCategory.Analytics  => "Analitik",
        CookieCategory.Marketing  => "Pazarlama",
        _                         => cat.ToString()
    };
}
