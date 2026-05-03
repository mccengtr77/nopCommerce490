using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.TurkeyCore.Components.CustomerTypeSelector;

/// <summary>
/// Bireysel/Kurumsal müşteri tipi seçici. Seçime göre TCKN (bireysel) veya
/// VKN + Vergi Dairesi + KEP (kurumsal) form alanlarının görünürlüğü JS ile değişir.
/// </summary>
public class CustomerTypeSelectorViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(
        string fieldName = "MusteriTipi",
        TurkishCustomerType selected = TurkishCustomerType.Individual)
    {
        var model = new CustomerTypeSelectorModel
        {
            FieldName = fieldName,
            Selected = selected
        };

        return View("~/Plugins/Misc.TurkeyCore/Components/CustomerTypeSelector/Default.cshtml", model);
    }
}

public class CustomerTypeSelectorModel
{
    public string FieldName { get; set; } = "MusteriTipi";
    public TurkishCustomerType Selected { get; set; } = TurkishCustomerType.Individual;
}
