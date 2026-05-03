using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Misc.TurkeyCore.Localization;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.TurkeyCore;

/// <summary>
/// TurkeyCore plugin — Türkiye e-ticaret çekirdek altyapı modülü.
///
/// Adres formundaki İl/İlçe/Mahalle + Müşteri Tipi (Bireysel/Kurumsal) +
/// TCKN/VKN/Vergi Dairesi alanları, plugin'in <c>Views/Shared/_CreateOrUpdateAddress.cshtml</c>
/// override'ı ile native template'e entegre edilir (PluginViewLocationExpander aracılığıyla).
/// </summary>
public class TurkeyCorePlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IScheduleTaskService _scheduleTaskService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public TurkeyCorePlugin(
        ILanguageService languageService,
        ILocalizationService localizationService,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _languageService = languageService;
        _localizationService = localizationService;
        _scheduleTaskService = scheduleTaskService;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Plugin yapılandırma sayfası URL'i
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/TurkeyCoreSettings/Configure";
    }

    /// <summary>
    /// Plugin kurulumu — ayarlar, lokalizasyon kaynakları ve zamanlanmış görevler
    /// </summary>
    public override async Task InstallAsync()
    {
        // Varsayılan ayarları kaydet
        await _settingService.SaveSettingAsync(new TurkeyCoreSettings());

        // TCMB döviz kuru güncelleme zamanlanmış görevi
        if (await _scheduleTaskService.GetTaskByTypeAsync(TurkeyCoreDefaults.Tcmb.ScheduleTaskType) is null)
        {
            await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
            {
                Name = TurkeyCoreDefaults.Tcmb.ScheduleTaskName,
                Type = TurkeyCoreDefaults.Tcmb.ScheduleTaskType,
                Seconds = TurkeyCoreDefaults.Tcmb.DefaultScheduleIntervalSeconds,
                Enabled = false,
                StopOnError = false
            });
        }

        // Türkçe (varsayılan dile yazılır)
        await _localizationService.AddOrUpdateLocaleResourceAsync(LocaleResources.Turkish);

        // İngilizce sürümünü mevcut İngilizce dilleri için kayıt et — yoksa atla
        var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);
        var englishLanguage = languages.FirstOrDefault(l =>
            l.UniqueSeoCode.Equals("en", StringComparison.OrdinalIgnoreCase));

        if (englishLanguage is not null)
            await _localizationService.AddOrUpdateLocaleResourceAsync(LocaleResources.English, englishLanguage.Id);

        await base.InstallAsync();
    }

    /// <summary>
    /// Plugin kaldırma — ayarlar, lokalizasyon kaynakları ve zamanlanmış görevleri temizle
    /// </summary>
    public override async Task UninstallAsync()
    {
        // Ayarları sil
        await _settingService.DeleteSettingAsync<TurkeyCoreSettings>();

        // Zamanlanmış görevi sil
        var task = await _scheduleTaskService.GetTaskByTypeAsync(TurkeyCoreDefaults.Tcmb.ScheduleTaskType);
        if (task is not null)
            await _scheduleTaskService.DeleteTaskAsync(task);

        // Tüm lokalizasyon kaynaklarını sil
        await _localizationService.DeleteLocaleResourcesAsync(TurkeyCoreDefaults.LocaleStringResourcesPrefix);

        await base.UninstallAsync();
    }

    #endregion
}
