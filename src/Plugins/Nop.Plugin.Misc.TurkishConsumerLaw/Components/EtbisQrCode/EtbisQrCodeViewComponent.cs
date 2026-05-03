using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Etbis;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Components.EtbisQrCode;

/// <summary>
/// ETBİS karekodu storefront footer'ında render eder.
/// 6563 sayılı E-Ticaret Kanunu ve ETBİS Yönetmeliği gereği zorunludur.
/// Kullanım:
///   @await Component.InvokeAsync("EtbisQrCode")
/// </summary>
public class EtbisQrCodeViewComponent : NopViewComponent
{
    protected readonly IEtbisService _etbisService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;

    public EtbisQrCodeViewComponent(
        IEtbisService etbisService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _etbisService = etbisService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<TurkishConsumerLawSettings>(store.Id);

        // Footer'da gösterim kapalıysa boş döner — yasal yükümlülük açısından
        // admin'in açık şekilde kapatması gerekir
        if (!settings.ShowEtbisQrCodeInFooter)
            return Content(string.Empty);

        var etbis = await _etbisService.GetActiveAsync(store.Id);
        if (etbis is null || string.IsNullOrWhiteSpace(etbis.QrCodeHtml))
            return Content(string.Empty);

        var model = new EtbisQrCodeModel(
            QrCodeHtml: etbis.QrCodeHtml,
            VerificationUrl: etbis.VerificationUrl,
            TradeName: etbis.TradeName);

        return View("~/Plugins/Misc.TurkishConsumerLaw/Components/EtbisQrCode/Default.cshtml", model);
    }
}

public record EtbisQrCodeModel(string QrCodeHtml, string? VerificationUrl, string? TradeName);
