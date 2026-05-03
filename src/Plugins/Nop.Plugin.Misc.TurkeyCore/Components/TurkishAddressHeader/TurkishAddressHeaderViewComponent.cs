using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.Address;
using Nop.Services.Directory;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Misc.TurkeyCore.Components.TurkishAddressHeader;

/// <summary>
/// Adres formunun en üstünde görünen header bölümü:
///  - Adres Adı (etiket: "Ev", "Ofis" vs.)
///  - Müşteri Tipi (Bireysel / Kurumsal radio)
///
/// Müşteri Tipi seçimi formdaki diğer alanların görünürlüğünü tetikler:
///  - Bireysel → Ad/Soyad + TCKN görünür, Ünvan (Company) + VKN gizli
///  - Kurumsal → Ünvan + VKN + Vergi Dairesi görünür, Ad/Soyad gizli
///    (form submit'te First/Last otomatik Company'den doldurulur — server validation)
///
/// Sadece Country=Turkey iken render olur.
/// </summary>
public class TurkishAddressHeaderViewComponent : NopViewComponent
{
    protected readonly ICountryService _countryService;
    protected readonly ITurkishAddressService _turkishAddressService;

    public TurkishAddressHeaderViewComponent(
        ICountryService countryService,
        ITurkishAddressService turkishAddressService)
    {
        _countryService = countryService;
        _turkishAddressService = turkishAddressService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone = "", object additionalData = null)
    {
        if (additionalData is not AddressModel addr)
            return Content(string.Empty);

        var turkey = await _countryService.GetCountryByTwoLetterIsoCodeAsync(TurkeyCoreDefaults.Country.TurkeyTwoLetterIsoCode);
        if (turkey is null)
            return Content(string.Empty);

        var initiallyHidden = addr.CountryId is not null && addr.CountryId != turkey.Id;

        var model = new TurkishAddressHeaderModel
        {
            FieldNamePrefix = "Address",
            TurkeyCountryId = turkey.Id,
            InitiallyHidden = initiallyHidden,
            MusteriTipi = TurkishCustomerType.Individual
        };

        if (addr.Id > 0)
        {
            var ext = await _turkishAddressService.GetByAddressIdAsync(addr.Id);
            if (ext is not null)
            {
                model.AdresAdi = ext.AdresAdi;
                model.MusteriTipi = ext.MusteriTipi;
            }
        }

        return View("~/Plugins/Misc.TurkeyCore/Components/TurkishAddressHeader/Default.cshtml", model);
    }
}

public class TurkishAddressHeaderModel
{
    public string FieldNamePrefix { get; set; } = "Address";
    public int TurkeyCountryId { get; set; }
    public bool InitiallyHidden { get; set; }
    public string? AdresAdi { get; set; }
    public TurkishCustomerType MusteriTipi { get; set; } = TurkishCustomerType.Individual;
}
