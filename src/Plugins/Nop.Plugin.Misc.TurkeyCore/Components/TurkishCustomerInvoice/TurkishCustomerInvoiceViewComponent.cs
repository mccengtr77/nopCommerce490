using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.Address;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Misc.TurkeyCore.Components.TurkishCustomerInvoice;

/// <summary>
/// Adres formuna gömülen müşteri tipi (Bireysel/Kurumsal) seçici ve seçime göre
/// koşullu görünen TCKN (bireysel) veya VKN+Vergi Dairesi (kurumsal) alanları.
///
/// Country=Turkey değilse boş render eder. ViewComponent <see cref="AddressModel"/>'i
/// <c>additionalData</c> olarak alır, edit modunda <see cref="TurkishAddressExtension"/>'daki
/// şifreli TCKN/VKN değerlerini decrypt edip input'a yerleştirir — kullanıcı kendi datasını
/// görmeyi hak ediyor (nopCommerce zaten address ownership kontrolü yapıyor: customer
/// sadece kendi adresini editleyebiliyor).
/// </summary>
public class TurkishCustomerInvoiceViewComponent : NopViewComponent
{
    protected readonly ICountryService _countryService;
    protected readonly IEncryptionService _encryptionService;
    protected readonly ITurkishAddressService _turkishAddressService;

    public TurkishCustomerInvoiceViewComponent(
        ICountryService countryService,
        IEncryptionService encryptionService,
        ITurkishAddressService turkishAddressService)
    {
        _countryService = countryService;
        _encryptionService = encryptionService;
        _turkishAddressService = turkishAddressService;
    }

    /// <param name="section">
    /// Hangi alanın render edileceğini seçer:
    ///  - "tckn"  → sadece TCKN (bireysel akış için, ad/soyad'tan sonra)
    ///  - "vergi" → sadece VKN + Vergi Dairesi (kurumsal akış için, formun sonunda)
    ///  - null/empty → her ikisi (geriye dönük uyumluluk)
    /// </param>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone = "", object additionalData = null, string section = null)
    {
        if (additionalData is not AddressModel addr)
            return Content(string.Empty);

        var turkey = await _countryService.GetCountryByTwoLetterIsoCodeAsync(TurkeyCoreDefaults.Country.TurkeyTwoLetterIsoCode);
        if (turkey is null)
            return Content(string.Empty);

        var initiallyHidden = addr.CountryId is not null && addr.CountryId != turkey.Id;

        var model = new TurkishCustomerInvoiceModel
        {
            FieldNamePrefix = "Address",
            TurkeyCountryId = turkey.Id,
            InitiallyHidden = initiallyHidden,
            MusteriTipi = TurkishCustomerType.Individual,
            Section = string.IsNullOrEmpty(section) ? "all" : section.ToLowerInvariant()
        };

        if (addr.Id > 0)
        {
            var ext = await _turkishAddressService.GetByAddressIdAsync(addr.Id);
            if (ext is not null)
            {
                model.MusteriTipi = ext.MusteriTipi;
                model.SelectedVergiDairesiId = ext.VergiDairesiId ?? 0;

                // Edit modunda kullanıcı kendi adresini açıyor — şifreli değerleri decrypt
                // edip input'ta plain göster. nopCommerce CustomerController.AddressEdit
                // customer-address ownership kontrolü yapıyor.
                model.ExistingTcKimlikNo = TryDecrypt(ext.TcKimlikNo);
                model.ExistingVergiNo = TryDecrypt(ext.VergiNo);
            }
        }

        return View("~/Plugins/Misc.TurkeyCore/Components/TurkishCustomerInvoice/Default.cshtml", model);
    }

    /// <summary>
    /// Şifreli değeri decrypt etmeye çalışır; başarısız olursa null döner (corrupt data,
    /// encryption key değişikliği veya plain text legacy değer durumlarına karşı sessiz fallback).
    /// </summary>
    private string? TryDecrypt(string? encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
            return null;
        try
        {
            return _encryptionService.DecryptText(encrypted);
        }
        catch
        {
            return null;
        }
    }
}

public class TurkishCustomerInvoiceModel
{
    public string FieldNamePrefix { get; set; } = "Address";
    public int TurkeyCountryId { get; set; }
    public bool InitiallyHidden { get; set; }
    public TurkishCustomerType MusteriTipi { get; set; } = TurkishCustomerType.Individual;
    public int SelectedVergiDairesiId { get; set; }

    /// <summary>Edit modunda decrypt edilmiş TCKN — input'a değer olarak yerleştirilir</summary>
    public string? ExistingTcKimlikNo { get; set; }

    /// <summary>Edit modunda decrypt edilmiş VKN/TCKN (kurumsal alan) — input'a değer olarak yerleştirilir</summary>
    public string? ExistingVergiNo { get; set; }

    /// <summary>"tckn" | "vergi" | "all"</summary>
    public string Section { get; set; } = "all";
    public bool ShowTckn => Section == "tckn" || Section == "all";
    public bool ShowVergi => Section == "vergi" || Section == "all";
}
