using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Misc.TurkeyCore.Areas.Admin.Components;
using Nop.Plugin.Misc.TurkeyCore.Components.ProductBaseCurrencyBadge;
using Nop.Plugin.Misc.TurkeyCore.Localization;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.TurkeyCore;

/// <summary>
/// TurkeyCore plugin — Türkiye e-ticaret çekirdek altyapı modülü.
///
/// Adres formundaki İl/İlçe/Mahalle + Müşteri Tipi (Bireysel/Kurumsal) +
/// TCKN/VKN/Vergi Dairesi alanları, plugin'in <c>Views/Shared/_CreateOrUpdateAddress.cshtml</c>
/// override'ı ile native template'e entegre edilir (PluginViewLocationExpander aracılığıyla).
///
/// Admin product edit page'inde "Türkiye - Döviz Bazlı Fiyat" panelini
/// <see cref="AdminWidgetZones.ProductDetailsBlock"/> zone'una <see cref="IWidgetPlugin"/>
/// üzerinden inject eder — admin USD/EUR/vb. base currency seçip BasePrice tanımlar,
/// storefront TCMB güncel kuru ile canlı TRY çevirimi yapar.
/// </summary>
public class TurkeyCorePlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
{
    #region Fields

    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IScheduleTaskService _scheduleTaskService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public TurkeyCorePlugin(
        ILanguageService languageService,
        ILocalizationService localizationService,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _languageService = languageService;
        _localizationService = localizationService;
        _scheduleTaskService = scheduleTaskService;
        _settingService = settingService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region IWidgetPlugin

    /// <summary>
    /// Plugin sadece admin product detail page'de bir widget zone'una hook olur.
    /// Frontend widget'ı yok — public store'da display için ayrı bir widget plugin
    /// (örn. Etbis QR) bu plugin tarafından sağlanmaz.
    /// </summary>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            // Admin product edit page'inde döviz tanımı paneli
            AdminWidgetZones.ProductDetailsBlock,
            // Storefront ürün detay sayfasında fiyatın hemen altı (Pavilion teması destekler)
            PublicWidgetZones.ProductPriceBottom,
            // Storefront kategori/listeleme product box'ında fiyat civarı (Pavilion teması destekler)
            PublicWidgetZones.ProductBoxAddinfoMiddle
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        ArgumentNullException.ThrowIfNull(widgetZone);

        if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
            return typeof(TurkishProductExtensionAdminViewComponent);

        if (widgetZone == PublicWidgetZones.ProductPriceBottom ||
            widgetZone == PublicWidgetZones.ProductBoxAddinfoMiddle)
            return typeof(ProductBaseCurrencyBadgeViewComponent);

        return null!;
    }

    /// <summary>
    /// Plugin widget listesinde gizlensin mi? Bu plugin <see cref="IMiscPlugin"/> kimliği taşıdığı
    /// için ana plugin listesinde görünür; widget listesinde ayrıca görünmesine gerek yok.
    /// </summary>
    public bool HideInWidgetList => true;

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

        // Widget zone'una hook olabilmek için plugin'in system name'i ActiveWidgetSystemNames'a eklenmeli
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(TurkeyCoreDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(TurkeyCoreDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

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
    /// Plugin sürüm güncellemesi (plugin.json Version değişimi).
    ///
    /// Update'te yapılması gerekenler:
    /// - Yeni eklenen widget zone hook'u için <see cref="WidgetSettings.ActiveWidgetSystemNames"/>'a
    ///   plugin'in system name'ini ekle (zaten install edilmiş plugin'de bu liste güncellenmemiş olabilir).
    /// - Yeni lokalizasyon string'lerini ekle (mevcut anahtarlara dokunmaz).
    /// - DB migration'ları nopCommerce <see cref="PluginService.UpdatePluginsAsync"/> tarafından
    ///   <see cref="InsertPluginData"/> üzerinden bu metoddan ÖNCE çalıştırılır — burada DB'ye dokunmaya gerek yok.
    /// </summary>
    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        // Widget zone'una hook olabilmek için plugin'in system name'i ActiveWidgetSystemNames'a eklenmeli.
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(TurkeyCoreDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(TurkeyCoreDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        // Yeni lokalizasyon string'leri (mevcutlar AddOrUpdate ile dokunulmaz, yeniler eklenir).
        await _localizationService.AddOrUpdateLocaleResourceAsync(LocaleResources.Turkish);

        var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);
        var englishLanguage = languages.FirstOrDefault(l =>
            l.UniqueSeoCode.Equals("en", StringComparison.OrdinalIgnoreCase));
        if (englishLanguage is not null)
            await _localizationService.AddOrUpdateLocaleResourceAsync(LocaleResources.English, englishLanguage.Id);

        await base.UpdateAsync(currentVersion, targetVersion);
    }

    /// <summary>
    /// Plugin kaldırma — ayarlar, lokalizasyon kaynakları ve zamanlanmış görevleri temizle
    /// </summary>
    public override async Task UninstallAsync()
    {
        // Ayarları sil
        await _settingService.DeleteSettingAsync<TurkeyCoreSettings>();

        // ActiveWidgetSystemNames'tan kaldır
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(TurkeyCoreDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(TurkeyCoreDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

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
