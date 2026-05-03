using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Components.KvkkDisclosureNotice;

/// <summary>
/// KVKK aydınlatma metni gösterici. Kayıt formunda checkbox'ların ÜSTÜNDE render edilir.
///
/// 2026/347 İlke Kararı: Aydınlatma metni içinde "rıza" ifadesi YOK; sadece
/// "Okudum ve anladım" beyanı var. Açık rıza onayları AYRI bölümde
/// (<see cref="ExplicitConsentCheckboxes.ExplicitConsentCheckboxesViewComponent"/>)
/// gösterilir.
///
/// Kullanım:
///   @await Component.InvokeAsync("KvkkDisclosureNotice")
/// </summary>
public class KvkkDisclosureNoticeViewComponent : NopViewComponent
{
    protected readonly IDisclosureTextService _disclosureService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;

    public KvkkDisclosureNoticeViewComponent(
        IDisclosureTextService disclosureService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _disclosureService = disclosureService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<TurkishConsumerLawSettings>(store.Id);

        var text = await _disclosureService.GetActiveAsync(store.Id);
        if (text is null)
            return Content(string.Empty);

        var model = new KvkkDisclosureNoticeModel(
            HtmlContent: text.Content,
            Version: text.Version,
            ReadAcknowledgmentRequired: settings.KvkkDisclosureMandatory);

        return View("~/Plugins/Misc.TurkishConsumerLaw/Components/KvkkDisclosureNotice/Default.cshtml", model);
    }
}

public record KvkkDisclosureNoticeModel(string HtmlContent, string Version, bool ReadAcknowledgmentRequired);
