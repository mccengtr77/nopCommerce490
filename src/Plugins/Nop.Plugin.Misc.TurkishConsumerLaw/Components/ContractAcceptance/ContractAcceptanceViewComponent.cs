using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Components.ContractAcceptance;

/// <summary>
/// Checkout son adımında MSS + ÖBF onay bölümünü render eder.
///
/// Mesafeli Sözleşmeler Yönetmeliği m.5: Tüketici, ÖBF'yi sipariş onayından önce
/// görmüş ve okumuş olmalı. MSS sipariş ile birlikte teslim edilir; iki onay AYRI
/// checkbox olarak alınır.
///
/// Kullanım:
///   @await Component.InvokeAsync("ContractAcceptance", new { previewMode = true })
///
/// Sözleşmeler henüz üretilmemiş olabilir (OrderPlacedEvent'ten önce).
/// previewMode=true → şablonların içeriğini anlık preview olarak gösterir
/// (token replace edilmemiş olabilir, sadece müşterinin "okumuş olma" durumu için).
/// </summary>
public class ContractAcceptanceViewComponent : NopViewComponent
{
    protected readonly IContractTemplateService _templateService;

    public ContractAcceptanceViewComponent(IContractTemplateService templateService)
    {
        _templateService = templateService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int storeId = 0, bool previewMode = true)
    {
        var mss = await _templateService.GetActiveAsync(ContractTemplateType.Mss, storeId);
        var obf = await _templateService.GetActiveAsync(ContractTemplateType.Obf, storeId);

        if (mss is null && obf is null)
            return Content(string.Empty); // şablon yoksa dummy render

        var model = new ContractAcceptanceModel
        {
            HasMss = mss is not null,
            HasObf = obf is not null,
            MssPreviewHtml = mss?.HtmlContent ?? string.Empty,
            ObfPreviewHtml = obf?.HtmlContent ?? string.Empty,
            PreviewMode = previewMode
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Components/ContractAcceptance/Default.cshtml", model);
    }
}

public class ContractAcceptanceModel
{
    public bool HasMss { get; set; }
    public bool HasObf { get; set; }
    public string MssPreviewHtml { get; set; } = string.Empty;
    public string ObfPreviewHtml { get; set; } = string.Empty;
    public bool PreviewMode { get; set; }
}
