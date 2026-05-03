using Microsoft.AspNetCore.Http;
using Nop.Core.Events;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.Address;
using Nop.Services.Events;
using NopAddress = Nop.Core.Domain.Common.Address;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Events;

/// <summary>
/// nopCommerce'in standart adres CRUD'unu (My account → Add new address, register, checkout)
/// dinleyip HTTP request form'undan İl/İlçe/Mahalle ID'lerini okur ve
/// <see cref="TurkishAddressExtension"/> tablosuna senkronize eder.
///
/// Form alanı isimleri ViewComponent'in kullandığı prefix ile eşleşir
/// (varsayılan: <c>Address.ProvinceId</c>, <c>Address.DistrictId</c>, <c>Address.NeighborhoodId</c>).
///
/// Native nopCommerce <see cref="Address.City"/> ve <see cref="Address.ZipPostalCode"/>
/// alanları storefront tarafında JS ile zaten doldurulduğundan, kargo/vergi modülleri
/// adresi tanımakta sorun yaşamaz.
/// </summary>
public class AddressLocationConsumer :
    IConsumer<EntityInsertedEvent<NopAddress>>,
    IConsumer<EntityUpdatedEvent<NopAddress>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITurkishAddressService _turkishAddressService;

    public AddressLocationConsumer(
        IHttpContextAccessor httpContextAccessor,
        ITurkishAddressService turkishAddressService)
    {
        _httpContextAccessor = httpContextAccessor;
        _turkishAddressService = turkishAddressService;
    }

    public Task HandleEventAsync(EntityInsertedEvent<NopAddress> eventMessage)
        => SyncFromRequestAsync(eventMessage.Entity);

    public Task HandleEventAsync(EntityUpdatedEvent<NopAddress> eventMessage)
        => SyncFromRequestAsync(eventMessage.Entity);

    private async Task SyncFromRequestAsync(NopAddress address)
    {
        if (address is null || address.Id <= 0)
            return;

        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is null || !ctx.Request.HasFormContentType)
            return;

        var form = ctx.Request.Form;

        var provinceId = ReadInt(form, "Address.ProvinceId", "ProvinceId", "BillingAddress.ProvinceId", "ShippingAddress.ProvinceId", "NewAddress.ProvinceId");
        var districtId = ReadInt(form, "Address.DistrictId", "DistrictId", "BillingAddress.DistrictId", "ShippingAddress.DistrictId", "NewAddress.DistrictId");
        var neighborhoodId = ReadInt(form, "Address.NeighborhoodId", "NeighborhoodId", "BillingAddress.NeighborhoodId", "ShippingAddress.NeighborhoodId", "NewAddress.NeighborhoodId");

        // Faturalama: müşteri tipi + TCKN/VKN/VergiDairesi + AdresAdi
        var musteriTipiRaw = ReadInt(form, "Address.MusteriTipi", "MusteriTipi", "BillingAddress.MusteriTipi", "ShippingAddress.MusteriTipi", "NewAddress.MusteriTipi");
        var tcKimlikNo = ReadString(form, "Address.TcKimlikNo", "TcKimlikNo", "BillingAddress.TcKimlikNo", "ShippingAddress.TcKimlikNo", "NewAddress.TcKimlikNo");
        var vergiNo = ReadString(form, "Address.VergiNo", "VergiNo", "BillingAddress.VergiNo", "ShippingAddress.VergiNo", "NewAddress.VergiNo");
        var vergiDairesiId = ReadInt(form, "Address.VergiDairesiId", "VergiDairesiId", "BillingAddress.VergiDairesiId", "ShippingAddress.VergiDairesiId", "NewAddress.VergiDairesiId");
        var adresAdi = ReadString(form, "Address.AdresAdi", "AdresAdi", "BillingAddress.AdresAdi", "ShippingAddress.AdresAdi", "NewAddress.AdresAdi");

        // Hiçbiri post edilmediyse Türkiye'ye özgü form değil — atla
        if (provinceId is null && districtId is null && neighborhoodId is null
            && musteriTipiRaw is null && tcKimlikNo is null && vergiNo is null && adresAdi is null)
            return;

        if (provinceId is not null || districtId is not null || neighborhoodId is not null)
        {
            await _turkishAddressService.SetLocationAsync(address, provinceId, districtId, neighborhoodId);
        }

        if (musteriTipiRaw is not null)
        {
            var musteriTipi = (TurkishCustomerType)musteriTipiRaw;
            await _turkishAddressService.SetInvoiceInfoAsync(address, musteriTipi, tcKimlikNo, vergiNo, vergiDairesiId);
        }

        if (adresAdi is not null)
        {
            await _turkishAddressService.SetAdresAdiAsync(address, adresAdi);
        }
    }

    private static string? ReadString(IFormCollection form, params string[] candidateKeys)
    {
        foreach (var key in candidateKeys)
        {
            if (form.TryGetValue(key, out var raw))
            {
                var trimmed = raw.ToString()?.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    return trimmed;
            }
        }
        return null;
    }

    private static int? ReadInt(IFormCollection form, params string[] candidateKeys)
    {
        foreach (var key in candidateKeys)
        {
            if (form.TryGetValue(key, out var raw) && int.TryParse(raw, out var v) && v > 0)
                return v;
        }
        return null;
    }
}
