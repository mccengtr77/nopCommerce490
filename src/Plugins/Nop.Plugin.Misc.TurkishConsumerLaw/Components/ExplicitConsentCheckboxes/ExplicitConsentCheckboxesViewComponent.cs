using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Components.ExplicitConsentCheckboxes;

/// <summary>
/// Açık rıza checkbox'ları — kayıt formunda KVKK aydınlatmasından AYRI bölümde gösterilir.
///
/// 2026/347 İlke Kararı: Tek kutucuk altında birden fazla rıza birleştirilemez.
/// ETK ticari ileti onayları KVKK rızalarından AYRI bölümde olur — `mode` parametresiyle ayrılır.
///
/// Kullanım:
///   @await Component.InvokeAsync("ExplicitConsentCheckboxes", new { mode = "kvkk" })
///   @await Component.InvokeAsync("ExplicitConsentCheckboxes", new { mode = "etk" })
/// </summary>
public class ExplicitConsentCheckboxesViewComponent : NopViewComponent
{
    protected readonly IExplicitConsentService _consentService;
    protected readonly IStoreContext _storeContext;

    public ExplicitConsentCheckboxesViewComponent(
        IExplicitConsentService consentService,
        IStoreContext storeContext)
    {
        _consentService = consentService;
        _storeContext = storeContext;
    }

    /// <param name="mode">"kvkk" (KVKK rızaları) veya "etk" (ticari ileti onayları)</param>
    public async Task<IViewComponentResult> InvokeAsync(string mode = "kvkk")
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var normalized = (mode ?? "kvkk").Trim().ToLowerInvariant();

        var texts = normalized == "etk"
            ? await _consentService.GetActiveEtkAsync(store.Id)
            : await _consentService.GetActiveKvkkAsync(store.Id);

        if (texts.Count == 0)
            return Content(string.Empty);

        var model = new ExplicitConsentCheckboxesModel(
            Mode: normalized,
            Title: normalized == "etk"
                ? "Ticari İleti Onayları"
                : "KVKK Açık Rıza Onayları",
            Items: texts);

        return View("~/Plugins/Misc.TurkishConsumerLaw/Components/ExplicitConsentCheckboxes/Default.cshtml", model);
    }
}

public record ExplicitConsentCheckboxesModel(
    string Mode,
    string Title,
    IList<ExplicitConsentText> Items);
