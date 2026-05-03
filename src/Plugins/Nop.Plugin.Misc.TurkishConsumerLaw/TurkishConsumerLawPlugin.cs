using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Misc.TurkishConsumerLaw.Localization;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.TurkishConsumerLaw;

/// <summary>
/// TurkishConsumerLaw — Türk tüketici hukuku uyumluluk paketi.
/// Bağımlılığı: Misc.TurkeyCore.
///
/// IWidgetPlugin implementasyonu sayesinde:
///  - Cookie consent banner her sayfada (body_start_html_tag_after) render edilir
///  - ETBİS QR code footer'da render edilir
/// </summary>
public class TurkishConsumerLawPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
{
    #region Fields

    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public TurkishConsumerLawPlugin(
        ILanguageService languageService,
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _languageService = languageService;
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/TurkishConsumerLawSettings/Configure";
    }

    #region IWidgetPlugin

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            // Cookie banner — her sayfada body açılışından hemen sonra
            PublicWidgetZones.BodyStartHtmlTagAfter,
            // ETBİS QR code — footer
            PublicWidgetZones.Footer
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.BodyStartHtmlTagAfter)
            return typeof(Components.CookieConsentBanner.CookieConsentBannerViewComponent);

        if (widgetZone == PublicWidgetZones.Footer)
            return typeof(Components.EtbisQrCode.EtbisQrCodeViewComponent);

        // Bilinmeyen zone — fallback (asla gerçekleşmez ama interface gerektirir)
        return typeof(Components.CookieConsentBanner.CookieConsentBannerViewComponent);
    }

    public bool HideInWidgetList => true;

    #endregion

    public override async Task InstallAsync()
    {
        // Default ayarlar
        await _settingService.SaveSettingAsync(new TurkishConsumerLawSettings());

        // Widget'ı aktif listeye ekle
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>();
        if (!widgetSettings.ActiveWidgetSystemNames.Contains(TurkishConsumerLawDefaults.SystemName))
        {
            widgetSettings.ActiveWidgetSystemNames.Add(TurkishConsumerLawDefaults.SystemName);
            await _settingService.SaveSettingAsync(widgetSettings);
        }

        // Lokalizasyon kaynakları
        await _localizationService.AddOrUpdateLocaleResourceAsync(LocaleResources.Turkish);

        var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);
        var englishLanguage = languages.FirstOrDefault(l =>
            l.UniqueSeoCode.Equals("en", StringComparison.OrdinalIgnoreCase));
        if (englishLanguage is not null)
            await _localizationService.AddOrUpdateLocaleResourceAsync(LocaleResources.English, englishLanguage.Id);

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Widget'ı aktif listeden çıkar
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>();
        if (widgetSettings.ActiveWidgetSystemNames.Contains(TurkishConsumerLawDefaults.SystemName))
        {
            widgetSettings.ActiveWidgetSystemNames.Remove(TurkishConsumerLawDefaults.SystemName);
            await _settingService.SaveSettingAsync(widgetSettings);
        }

        await _settingService.DeleteSettingAsync<TurkishConsumerLawSettings>();
        await _localizationService.DeleteLocaleResourcesAsync(TurkishConsumerLawDefaults.LocaleStringResourcesPrefix);
        await base.UninstallAsync();
    }

    #endregion
}
