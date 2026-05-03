using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Components.DataSubjectRequestForm;

/// <summary>
/// KVKK m.11 başvuru formu — public sayfada (örn. /kvkk-basvuru) gösterilir.
/// Kayıtlı müşteri ise ad/e-posta otomatik doldurulur, anonim ise boş.
/// </summary>
public class DataSubjectRequestFormViewComponent : NopViewComponent
{
    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;

    public DataSubjectRequestFormViewComponent(
        IWorkContext workContext,
        ICustomerService customerService)
    {
        _workContext = workContext;
        _customerService = customerService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var isRegistered = customer is not null && await _customerService.IsRegisteredAsync(customer);

        var model = new DataSubjectRequestFormModel
        {
            DefaultFullName = isRegistered
                ? $"{customer!.FirstName} {customer.LastName}".Trim()
                : string.Empty,
            DefaultEmail = isRegistered ? customer!.Email ?? string.Empty : string.Empty,
            RequestTypes = Enum.GetValues<DataSubjectRequestType>()
                .Select(t => new RequestTypeOption((int)t, GetLabel(t)))
                .ToList()
        };

        return View("~/Plugins/Misc.TurkishConsumerLaw/Components/DataSubjectRequestForm/Default.cshtml", model);
    }

    private static string GetLabel(DataSubjectRequestType type) => type switch
    {
        DataSubjectRequestType.Inquire                 => "İşlenip işlenmediğini öğrenme (m.11/a)",
        DataSubjectRequestType.Access                  => "İşleme bilgisi talep etme (m.11/b)",
        DataSubjectRequestType.PurposeInfo             => "İşleme amacını öğrenme (m.11/c)",
        DataSubjectRequestType.Rectification           => "Düzeltme (m.11/d)",
        DataSubjectRequestType.Erasure                 => "Silme/Yok etme (m.11/e)",
        DataSubjectRequestType.NotifyThirdParties      => "3. taraflara bildirim (m.11/f)",
        DataSubjectRequestType.ObjectAutomatedDecision => "Otomatik karar itirazı (m.11/g)",
        DataSubjectRequestType.Compensation            => "Tazminat (m.11/h)",
        DataSubjectRequestType.Portability             => "Veri taşınabilirliği",
        _                                              => type.ToString()
    };
}

public class DataSubjectRequestFormModel
{
    public string DefaultFullName { get; set; } = string.Empty;
    public string DefaultEmail { get; set; } = string.Empty;
    public IList<RequestTypeOption> RequestTypes { get; set; } = new List<RequestTypeOption>();
}

public record RequestTypeOption(int Value, string Label);
