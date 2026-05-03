using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Components.CookieConsentBanner;

/// <summary>
/// Çerez consent banner — kullanıcı tarayıcıya consent cookie'si yazılmadıysa
/// ekran üst/altında görüntülenir. Storefront layout'a şu şekilde eklenir:
///   @await Component.InvokeAsync("CookieConsentBanner")
///
/// Kategori bazlı toggle'lar (Functional/Analytics/Marketing) + Kabul Et / Reddet / Tercihleri Kaydet.
/// </summary>
public class CookieConsentBannerViewComponent : NopViewComponent
{
    protected readonly ICookieDefinitionService _definitionService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;

    public CookieConsentBannerViewComponent(
        ICookieDefinitionService definitionService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _definitionService = definitionService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<TurkishConsumerLawSettings>(store.Id);

        if (!settings.CookieConsentEnabled)
            return Content(string.Empty);

        // Mevcut cookie consent var mı? Banner'ı render et — JS tarafı consent cookie'sini
        // okuyup banner'ı gizler/gösterir. Bu sayede sayfa cache'lenebilir kalır.
        // (Server-side cookie kontrolü yapsaydık her sayfanın output'u değişirdi.)

        var definitions = await _definitionService.GetActiveAsync();
        var groups = definitions
            .GroupBy(c => c.Category)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());

        var model = new CookieConsentBannerModel
        {
            Position = settings.CookieBannerPosition ?? "bottom",
            ConsentCookieName = TurkishConsumerLawDefaults.Cookies.ConsentCookieName,
            PolicyVersion = TurkishConsumerLawDefaults.Cookies.CurrentPolicyVersion,
            Necessary = groups.GetValueOrDefault(CookieCategory.Necessary) ?? new(),
            Functional = groups.GetValueOrDefault(CookieCategory.Functional) ?? new(),
            Analytics = groups.GetValueOrDefault(CookieCategory.Analytics) ?? new(),
            Marketing = groups.GetValueOrDefault(CookieCategory.Marketing) ?? new()
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Components/CookieConsentBanner/Default.cshtml", model);
    }
}

public class CookieConsentBannerModel
{
    public string Position { get; set; } = "bottom";
    public string ConsentCookieName { get; set; } = string.Empty;
    public string PolicyVersion { get; set; } = "1.0";
    public List<CookieDefinition> Necessary { get; set; } = new();
    public List<CookieDefinition> Functional { get; set; } = new();
    public List<CookieDefinition> Analytics { get; set; } = new();
    public List<CookieDefinition> Marketing { get; set; } = new();
}
