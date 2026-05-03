using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Areas.Admin.Models;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.TurkeyCore.Areas.Admin.Controllers;

/// <summary>
/// TurkeyCore plugin Configure sayfası — sadece admin erişimli.
/// nopCommerce convention: AuthorizeAdmin + AreaAdmin + AntiForgery.
/// </summary>
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class TurkeyCoreSettingsController : BasePluginController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;
    protected readonly ITcmbExchangeRateService _exchangeRateService;
    protected readonly IRepository<TurkishProvince> _provinceRepository;
    protected readonly IRepository<TurkishDistrict> _districtRepository;
    protected readonly IRepository<TurkishNeighborhood> _neighborhoodRepository;
    protected readonly IRepository<TurkishTaxOffice> _taxOfficeRepository;
    protected readonly IRepository<ExchangeRateLog> _exchangeRateRepository;

    #endregion

    #region Ctor

    public TurkeyCoreSettingsController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext,
        ITcmbExchangeRateService exchangeRateService,
        IRepository<TurkishProvince> provinceRepository,
        IRepository<TurkishDistrict> districtRepository,
        IRepository<TurkishNeighborhood> neighborhoodRepository,
        IRepository<TurkishTaxOffice> taxOfficeRepository,
        IRepository<ExchangeRateLog> exchangeRateRepository)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
        _exchangeRateService = exchangeRateService;
        _provinceRepository = provinceRepository;
        _districtRepository = districtRepository;
        _neighborhoodRepository = neighborhoodRepository;
        _taxOfficeRepository = taxOfficeRepository;
        _exchangeRateRepository = exchangeRateRepository;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        var storeId = (await _storeContext.GetActiveStoreScopeConfigurationAsync());
        var settings = await _settingService.LoadSettingAsync<TurkeyCoreSettings>(storeId);

        var model = new ConfigurationModel
        {
            TcmbAutoUpdateEnabled = settings.TcmbAutoUpdateEnabled,
            GibMukellefServiceUrl = settings.GibMukellefServiceUrl,
            GibCacheTimeMinutes = settings.GibCacheTimeMinutes,
            TcknRequiredOnRegistration = settings.TcknRequiredOnRegistration,
            VknRequiredForCorporate = settings.VknRequiredForCorporate,
            LocationSelectionRequired = settings.LocationSelectionRequired,
            DefaultVatRate = settings.DefaultVatRate
        };

        // Veri durumu özeti — admin'e seed durumunu göster
        model.DataStatus = new DataStatusModel
        {
            ProvinceCount = _provinceRepository.Table.Count(),
            DistrictCount = _districtRepository.Table.Count(),
            NeighborhoodCount = _neighborhoodRepository.Table.Count(),
            TaxOfficeCount = _taxOfficeRepository.Table.Count()
        };

        // Son TCMB kurları — top 30 en güncel
        model.LatestRates = await _exchangeRateRepository.GetAllAsync(
            query => query.OrderByDescending(r => r.RateDateUtc)
                          .ThenBy(r => r.CurrencyCode)
                          .Take(30),
            getCacheKey: null);

        return View("~/Plugins/Misc.TurkeyCore/Areas/Admin/Views/Configure.cshtml", model);
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    public async Task<IActionResult> ConfigurePost(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Configure();

        var storeId = (await _storeContext.GetActiveStoreScopeConfigurationAsync());
        var settings = await _settingService.LoadSettingAsync<TurkeyCoreSettings>(storeId);

        settings.TcmbAutoUpdateEnabled = model.TcmbAutoUpdateEnabled;
        settings.GibMukellefServiceUrl = model.GibMukellefServiceUrl ?? string.Empty;
        settings.GibCacheTimeMinutes = model.GibCacheTimeMinutes;
        settings.TcknRequiredOnRegistration = model.TcknRequiredOnRegistration;
        settings.VknRequiredForCorporate = model.VknRequiredForCorporate;
        settings.LocationSelectionRequired = model.LocationSelectionRequired;
        settings.DefaultVatRate = model.DefaultVatRate;

        await _settingService.SaveSettingAsync(settings, storeId);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction("Configure");
    }

    /// <summary>
    /// "Şimdi Güncelle" butonu — TCMB feed'ini elle çeker.
    /// </summary>
    [HttpPost, ActionName("Configure")]
    [FormValueRequired("refresh-rates")]
    public async Task<IActionResult> RefreshRates()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        try
        {
            var count = await _exchangeRateService.RefreshRatesAsync();
            _notificationService.SuccessNotification(
                $"TCMB kurları güncellendi: {count} kayıt eklendi");
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification($"TCMB güncellemesi başarısız: {ex.Message}");
        }

        return RedirectToAction("Configure");
    }

    #endregion
}
