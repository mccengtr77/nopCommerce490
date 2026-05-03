using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkeyCore.Models;
using Nop.Plugin.Misc.TurkeyCore.Services.Address;
using Nop.Plugin.Misc.TurkeyCore.Services.Location;
using Nop.Services.Directory;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Misc.TurkeyCore.Components.ProvinceDistrictSelector;

/// <summary>
/// Storefront'ta İl/İlçe/Mahalle cascading dropdown'larını render eden ViewComponent.
///
/// İki şekilde çağrılır:
/// 1) <b>Widget zone üzerinden</b> (otomatik) — nopCommerce, <c>address_bottom</c>
///    zone'unda <see cref="WidgetViewComponent"/> aracılığıyla bu component'i tetikler.
///    <c>additionalData</c> olarak <see cref="AddressModel"/> geçer.
/// 2) <b>Manuel</b> — Razor view'dan
///    <c>@await Component.InvokeAsync("ProvinceDistrictSelector", new { fieldNamePrefix = "Address" })</c>.
///
/// Country=Turkey değilse boş render eder; yani widget zone her zaman tetiklense de
/// sadece adres ülkesi Türkiye olduğunda Türkçe alanlar görünür.
/// </summary>
public class ProvinceDistrictSelectorViewComponent : NopViewComponent
{
    protected readonly ICountryService _countryService;
    protected readonly ITurkishAddressService _turkishAddressService;
    protected readonly ITurkishLocationService _locationService;

    public ProvinceDistrictSelectorViewComponent(
        ICountryService countryService,
        ITurkishAddressService turkishAddressService,
        ITurkishLocationService locationService)
    {
        _countryService = countryService;
        _turkishAddressService = turkishAddressService;
        _locationService = locationService;
    }

    /// <param name="widgetZone">
    /// Widget framework'ünün geçtiği zone adı (manuel çağrıda boş gelir, sorun değil).
    /// </param>
    /// <param name="additionalData">
    /// Widget zone üzerinden çağrıldığında <see cref="AddressModel"/> nesnesi.
    /// Manuel çağrıda override parametreleri (anonymous tip) burada gelir.
    /// </param>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone = "", object additionalData = null)
    {
        // 1) Country=Turkey kontrolü — değilse boş render
        var (countryId, addressId, fieldPrefix, selectedProvinceId, selectedDistrictId, selectedNeighborhoodId) =
            ExtractContext(additionalData);

        var turkey = await _countryService.GetCountryByTwoLetterIsoCodeAsync(TurkeyCoreDefaults.Country.TurkeyTwoLetterIsoCode);
        if (turkey is null)
            return Content(string.Empty); // Türkiye DB'de yok — sessiz no-op

        // additionalData (widget) varsa CountryId Turkey değilse boş render —
        // ama JS yine de country select'in change event'inde göster/gizle yapacak.
        // İlk render'da Turkey değilse hidden başlat (CSS ile).
        var initiallyHidden = countryId is not null && countryId != turkey.Id;

        // 2) Mevcut adres için kayıtlı extension'dan seçimleri yükle (edit modu)
        if (addressId > 0 && selectedProvinceId == 0)
        {
            var ext = await _turkishAddressService.GetByAddressIdAsync(addressId);
            if (ext is not null)
            {
                selectedProvinceId = ext.ProvinceId ?? 0;
                selectedDistrictId = ext.DistrictId ?? 0;
                selectedNeighborhoodId = ext.NeighborhoodId ?? 0;
            }
        }

        // 3) İl listesi her zaman yüklenir (cache'lenmiş, ucuz)
        var provinces = await _locationService.GetAllProvincesAsync();

        var model = new ProvinceDistrictSelectorModel
        {
            FieldNamePrefix = fieldPrefix,
            SelectedProvinceId = selectedProvinceId,
            SelectedDistrictId = selectedDistrictId,
            SelectedNeighborhoodId = selectedNeighborhoodId,
            TurkeyCountryId = turkey.Id,
            InitiallyHidden = initiallyHidden,
            Provinces = provinces
                .Select(p => new ProvinceListItemModel(p.Id, p.Name, p.PlateCode))
                .ToList()
        };

        // 4) Edit modunda seçili il/ilçe varsa cascading'in ilk seviyelerini server-side doldur
        if (selectedProvinceId > 0)
        {
            var districts = await _locationService.GetDistrictsByProvinceIdAsync(selectedProvinceId);
            model.Districts = districts
                .Select(d => new DistrictListItemModel(d.Id, d.Name))
                .ToList();
        }

        if (selectedDistrictId > 0)
        {
            var neighborhoods = await _locationService.GetNeighborhoodsByDistrictIdAsync(selectedDistrictId);
            model.Neighborhoods = neighborhoods
                .Select(n => new NeighborhoodListItemModel(n.Id, n.Name, n.PostalCode))
                .ToList();
        }

        return View("~/Plugins/Misc.TurkeyCore/Components/ProvinceDistrictSelector/Default.cshtml", model);
    }

    /// <summary>
    /// <c>additionalData</c>'yı tanımaya çalışır. AddressModel ise alanları okur,
    /// anonymous tip ise reflection ile parametreleri çeker, null/diğer tipler için defaultları döner.
    /// </summary>
    private static (int? CountryId, int AddressId, string FieldPrefix,
        int SelectedProvinceId, int SelectedDistrictId, int SelectedNeighborhoodId)
        ExtractContext(object additionalData)
    {
        const string defaultPrefix = "Address";

        if (additionalData is AddressModel addr)
        {
            return (
                addr.CountryId,
                addr.Id,
                defaultPrefix,
                0, 0, 0); // edit modu: extension tablosundan yükle
        }

        // Manuel çağrı (anonymous) — reflection ile çek
        if (additionalData is not null)
        {
            var t = additionalData.GetType();
            int? cid = (int?)t.GetProperty("countryId")?.GetValue(additionalData);
            int aid = (int?)t.GetProperty("addressId")?.GetValue(additionalData) ?? 0;
            string prefix = (string)t.GetProperty("fieldNamePrefix")?.GetValue(additionalData) ?? defaultPrefix;
            int pid = (int?)t.GetProperty("selectedProvinceId")?.GetValue(additionalData) ?? 0;
            int did = (int?)t.GetProperty("selectedDistrictId")?.GetValue(additionalData) ?? 0;
            int nid = (int?)t.GetProperty("selectedNeighborhoodId")?.GetValue(additionalData) ?? 0;
            return (cid, aid, prefix, pid, did, nid);
        }

        return (null, 0, defaultPrefix, 0, 0, 0);
    }
}

/// <summary>
/// ViewComponent'e geçilen model.
/// </summary>
public class ProvinceDistrictSelectorModel
{
    public string FieldNamePrefix { get; set; } = string.Empty;
    public int SelectedProvinceId { get; set; }
    public int SelectedDistrictId { get; set; }
    public int SelectedNeighborhoodId { get; set; }

    /// <summary>nopCommerce DB'sindeki Türkiye Country.Id — JS, country select değişince bunu kontrol eder</summary>
    public int TurkeyCountryId { get; set; }

    /// <summary>İlk render'da bu component <c>display:none</c> başlasın mı (mevcut adres Türkiye değilse)</summary>
    public bool InitiallyHidden { get; set; }

    public IList<ProvinceListItemModel> Provinces { get; set; } = new List<ProvinceListItemModel>();
    public IList<DistrictListItemModel> Districts { get; set; } = new List<DistrictListItemModel>();
    public IList<NeighborhoodListItemModel> Neighborhoods { get; set; } = new List<NeighborhoodListItemModel>();
}
