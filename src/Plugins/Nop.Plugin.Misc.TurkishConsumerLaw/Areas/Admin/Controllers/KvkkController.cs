using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;
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
public class KvkkController : BasePluginController
{
    protected readonly IDisclosureTextService _disclosureService;
    protected readonly IExplicitConsentService _consentService;
    protected readonly IPermissionService _permissionService;
    protected readonly INotificationService _notificationService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IStoreContext _storeContext;

    public KvkkController(
        IDisclosureTextService disclosureService,
        IExplicitConsentService consentService,
        IPermissionService permissionService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IStoreContext storeContext)
    {
        _disclosureService = disclosureService;
        _consentService = consentService;
        _permissionService = permissionService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _storeContext = storeContext;
    }

    public async Task<IActionResult> Texts()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var disclosure = await _disclosureService.GetActiveAsync(storeId);
        var consents = await _consentService.GetActiveAsync(storeId);

        var model = new KvkkTextsListModel
        {
            Disclosure = disclosure is null ? null : new DisclosureTextModel
            {
                Id = disclosure.Id,
                Title = disclosure.Title,
                Content = disclosure.Content,
                Version = disclosure.Version,
                IsActive = disclosure.IsActive
            },
            ExplicitConsents = consents.Select(c => new ExplicitConsentRowModel
            {
                Id = c.Id,
                Scope = c.Scope,
                ScopeLabel = GetScopeLabel(c.Scope),
                ShortLabel = c.ShortLabel,
                Version = c.Version,
                DefaultChecked = c.DefaultChecked,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder
            }).ToList()
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Kvkk/Texts.cshtml", model);
    }

    [HttpPost, ActionName("Texts")]
    [FormValueRequired("save-disclosure")]
    public async Task<IActionResult> SaveDisclosure(KvkkTextsListModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (model.Disclosure is null)
            return RedirectToAction("Texts");

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var existing = model.Disclosure.Id > 0
            ? await _disclosureService.GetByIdAsync(model.Disclosure.Id)
            : null;

        if (existing is null)
        {
            await _disclosureService.InsertAsync(new DisclosureText
            {
                Title = model.Disclosure.Title,
                Content = model.Disclosure.Content,
                Version = model.Disclosure.Version,
                IsActive = model.Disclosure.IsActive,
                LimitedToStoreId = storeId
            });
        }
        else
        {
            existing.Title = model.Disclosure.Title;
            existing.Content = model.Disclosure.Content;
            existing.Version = model.Disclosure.Version;
            existing.IsActive = model.Disclosure.IsActive;
            await _disclosureService.UpdateAsync(existing);
        }

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Common.DataEditedSuccessfully"));

        return RedirectToAction("Texts");
    }

    public async Task<IActionResult> EditConsent(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var consent = await _consentService.GetByIdAsync(id);
        if (consent is null)
            return RedirectToAction("Texts");

        var model = new ExplicitConsentEditModel
        {
            Id = consent.Id,
            Scope = consent.Scope,
            ShortLabel = consent.ShortLabel,
            Content = consent.Content,
            Version = consent.Version,
            DefaultChecked = consent.DefaultChecked,
            DisplayOrder = consent.DisplayOrder,
            IsActive = consent.IsActive
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Kvkk/EditConsent.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditConsent(ExplicitConsentEditModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var consent = await _consentService.GetByIdAsync(model.Id);
        if (consent is null)
            return RedirectToAction("Texts");

        consent.ShortLabel = model.ShortLabel;
        consent.Content = model.Content;
        consent.Version = model.Version;
        consent.DefaultChecked = model.DefaultChecked;
        consent.DisplayOrder = model.DisplayOrder;
        consent.IsActive = model.IsActive;

        await _consentService.UpdateAsync(consent);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Common.DataEditedSuccessfully"));

        return RedirectToAction("Texts");
    }

    private static string GetScopeLabel(ConsentScope scope) => scope switch
    {
        ConsentScope.KvkkPersonalData       => "KVKK — Kişisel veri",
        ConsentScope.KvkkProfiling          => "KVKK — Profilleme",
        ConsentScope.KvkkThirdPartyDomestic => "KVKK — 3. taraf (yurt içi)",
        ConsentScope.KvkkOverseasTransfer   => "KVKK — Yurt dışı aktarım (m.9)",
        ConsentScope.EtkSms                 => "ETK — SMS",
        ConsentScope.EtkEmail               => "ETK — E-posta",
        ConsentScope.EtkCall                => "ETK — Arama",
        _                                   => scope.ToString()
    };
}
