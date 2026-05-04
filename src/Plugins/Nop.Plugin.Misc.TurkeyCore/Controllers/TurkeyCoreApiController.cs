using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.TurkeyCore.Models;
using Nop.Plugin.Misc.TurkeyCore.Services.Location;
using Nop.Plugin.Misc.TurkeyCore.Services.TaxOffice;
using Nop.Plugin.Misc.TurkeyCore.Services.Validation;

namespace Nop.Plugin.Misc.TurkeyCore.Controllers;

/// <summary>
/// Storefront'taki cascading dropdown'lar ve real-time TCKN/VKN doğrulaması için
/// JSON döndüren public AJAX endpoint'leri.
///
/// Auth gerekmez — il/ilçe/mahalle public veridir, validasyon ise pure fonksiyondur.
/// Anti-forgery POST endpoint'leri için aktif (default).
/// </summary>
[Route("Plugins/TurkeyCore/api")]
[ApiController]
public class TurkeyCoreApiController : ControllerBase
{
    #region Fields

    protected readonly ITurkishLocationService _locationService;
    protected readonly ITurkishTaxOfficeService _taxOfficeService;
    protected readonly ITurkishValidationService _validationService;

    #endregion

    #region Ctor

    public TurkeyCoreApiController(
        ITurkishLocationService locationService,
        ITurkishTaxOfficeService taxOfficeService,
        ITurkishValidationService validationService)
    {
        _locationService = locationService;
        _taxOfficeService = taxOfficeService;
        _validationService = validationService;
    }

    #endregion

    #region Lokasyon

    /// <summary>
    /// GET /Plugins/TurkeyCore/api/provinces — tüm aktif iller
    /// </summary>
    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvinces()
    {
        var provinces = await _locationService.GetAllProvincesAsync();
        // Anonymous obj kullan — nopCommerce'in JSON serializer default'u PascalCase'i koruyor;
        // storefront JS camelCase bekliyor. Anonymous obj'un property adları lowercase tanımlanır.
        var result = provinces.Select(p => new { id = p.Id, name = p.Name, plateCode = p.PlateCode }).ToArray();
        return Ok(result);
    }

    /// <summary>
    /// GET /Plugins/TurkeyCore/api/districts/{provinceId}
    /// </summary>
    [HttpGet("districts/{provinceId:int}")]
    public async Task<IActionResult> GetDistricts(int provinceId)
    {
        var districts = await _locationService.GetDistrictsByProvinceIdAsync(provinceId);
        var result = districts.Select(d => new { id = d.Id, name = d.Name }).ToArray();
        return Ok(result);
    }

    /// <summary>
    /// GET /Plugins/TurkeyCore/api/neighborhoods/{districtId}
    /// </summary>
    [HttpGet("neighborhoods/{districtId:int}")]
    public async Task<IActionResult> GetNeighborhoods(int districtId)
    {
        var neighborhoods = await _locationService.GetNeighborhoodsByDistrictIdAsync(districtId);
        var result = neighborhoods
            .Select(n => new { id = n.Id, name = n.Name, postalCode = n.PostalCode })
            .ToArray();
        return Ok(result);
    }

    /// <summary>
    /// GET /Plugins/TurkeyCore/api/tax-offices/{provinceId} — bir il'e bağlı vergi daireleri.
    /// </summary>
    [HttpGet("tax-offices/{provinceId:int}")]
    public async Task<IActionResult> GetTaxOffices(int provinceId)
    {
        var offices = await _taxOfficeService.GetByProvinceIdAsync(provinceId);
        var result = offices.Select(o => new { id = o.Id, name = o.Name, code = o.Code }).ToArray();
        return Ok(result);
    }

    #endregion

    #region Validasyon

    public record ValidateTcknRequest(string Tckn);
    public record ValidateVknRequest(string Vkn);

    /// <summary>
    /// POST /Plugins/TurkeyCore/api/validate-tckn
    /// Body: { "tckn": "12345678950" }
    /// </summary>
    [HttpPost("validate-tckn")]
    public IActionResult ValidateTckn([FromBody] ValidateTcknRequest request)
    {
        var isValid = _validationService.ValidateTcKimlikNo(request?.Tckn);
        return Ok(new ValidationResultModel(isValid,
            isValid ? null : "Geçersiz TC Kimlik No"));
    }

    /// <summary>
    /// POST /Plugins/TurkeyCore/api/validate-vkn
    /// </summary>
    [HttpPost("validate-vkn")]
    public IActionResult ValidateVkn([FromBody] ValidateVknRequest request)
    {
        var isValid = _validationService.ValidateVergiNo(request?.Vkn);
        return Ok(new ValidationResultModel(isValid,
            isValid ? null : "Geçersiz Vergi Kimlik No"));
    }

    #endregion
}
