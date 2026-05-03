using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Etbis;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Etbis;
using Nop.Services.Configuration;
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
public class TurkishConsumerLawSettingsController : BasePluginController
{
    #region Fields

    protected readonly IEtbisService _etbisService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public TurkishConsumerLawSettingsController(
        IEtbisService etbisService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _etbisService = etbisService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<TurkishConsumerLawSettings>(storeId);

        var activeEtbis = await _etbisService.GetActiveAsync(storeId);
        var etbisModel = activeEtbis is null
            ? new EtbisRegistrationModel()
            : new EtbisRegistrationModel
            {
                Id = activeEtbis.Id,
                MersisNo = activeEtbis.MersisNo,
                TradeName = activeEtbis.TradeName,
                RegistrationDate = activeEtbis.RegistrationDate,
                VerificationUrl = activeEtbis.VerificationUrl,
                QrCodeHtml = activeEtbis.QrCodeHtml,
                IsActive = activeEtbis.IsActive
            };

        var model = new ConfigurationModel
        {
            ShowEtbisQrCodeInFooter = settings.ShowEtbisQrCodeInFooter,
            ActiveEtbis = etbisModel,
            KvkkDisclosureMandatory = settings.KvkkDisclosureMandatory,
            EnforceSeparateDisclosureAndConsent = settings.EnforceSeparateDisclosureAndConsent,
            CookieConsentEnabled = settings.CookieConsentEnabled,
            CookieBannerPosition = settings.CookieBannerPosition,
            WithdrawalPeriodDays = settings.WithdrawalPeriodDays,
            ReturnShippingPaidBySeller = settings.ReturnShippingPaidBySeller
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Areas/Admin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Configure();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<TurkishConsumerLawSettings>(storeId);

        // Settings güncelle
        settings.ShowEtbisQrCodeInFooter = model.ShowEtbisQrCodeInFooter;
        settings.KvkkDisclosureMandatory = model.KvkkDisclosureMandatory;
        settings.EnforceSeparateDisclosureAndConsent = model.EnforceSeparateDisclosureAndConsent;
        settings.CookieConsentEnabled = model.CookieConsentEnabled;
        settings.CookieBannerPosition = model.CookieBannerPosition ?? "bottom";
        settings.WithdrawalPeriodDays = model.WithdrawalPeriodDays;
        settings.ReturnShippingPaidBySeller = model.ReturnShippingPaidBySeller;
        await _settingService.SaveSettingAsync(settings, storeId);

        // ETBİS upsert — admin tek formdan editler, eski kayıt güncellenir veya yeni eklenir
        if (!string.IsNullOrWhiteSpace(model.ActiveEtbis.MersisNo)
            && !string.IsNullOrWhiteSpace(model.ActiveEtbis.TradeName))
        {
            await UpsertEtbisAsync(model.ActiveEtbis, storeId);
        }

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction("Configure");
    }

    private async Task UpsertEtbisAsync(EtbisRegistrationModel m, int storeId)
    {
        var existing = m.Id > 0 ? await _etbisService.GetByIdAsync(m.Id) : null;

        if (existing is null)
        {
            await _etbisService.InsertAsync(new EtbisRegistration
            {
                MersisNo = m.MersisNo,
                TradeName = m.TradeName,
                RegistrationDate = m.RegistrationDate,
                VerificationUrl = m.VerificationUrl,
                QrCodeHtml = m.QrCodeHtml ?? string.Empty,
                LimitedToStoreId = storeId,
                IsActive = m.IsActive
            });
        }
        else
        {
            existing.MersisNo = m.MersisNo;
            existing.TradeName = m.TradeName;
            existing.RegistrationDate = m.RegistrationDate;
            existing.VerificationUrl = m.VerificationUrl;
            existing.QrCodeHtml = m.QrCodeHtml ?? string.Empty;
            existing.IsActive = m.IsActive;
            await _etbisService.UpdateAsync(existing);
        }
    }

    #endregion
}
