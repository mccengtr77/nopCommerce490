using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Controllers;

/// <summary>
/// Çerez consent storefront AJAX endpoint'leri.
///
/// Banner'daki "Kabul Et" / "Tercihleri Kaydet" butonu bu endpoint'i çağırır.
/// Endpoint server-side audit kaydı oluşturur ve tarayıcıya `tcl-consent` cookie'sini yazar.
/// </summary>
[Route("Plugins/TurkishConsumerLaw/api/cookie-consent")]
[ApiController]
public class CookieConsentController : ControllerBase
{
    #region Fields

    protected readonly ICookieConsentService _consentService;
    protected readonly IWorkContext _workContext;
    protected readonly IWebHelper _webHelper;
    protected readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public CookieConsentController(
        ICookieConsentService consentService,
        IWorkContext workContext,
        IWebHelper webHelper,
        ICustomerService customerService)
    {
        _consentService = consentService;
        _workContext = workContext;
        _webHelper = webHelper;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    public record SaveConsentRequest(bool Functional, bool Analytics, bool Marketing);

    public record SaveConsentResponse(bool Success, string ConsentGuid, string PolicyVersion);

    /// <summary>
    /// POST /Plugins/TurkishConsumerLaw/api/cookie-consent
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Save([FromBody] SaveConsentRequest request)
    {
        if (request is null)
            return BadRequest();

        // Mevcut consent cookie'si varsa GUID'i koru, yoksa yeni üret
        var consentGuid = ReadOrCreateGuid();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerId = customer is not null && await _customerService.IsRegisteredAsync(customer)
            ? customer.Id
            : 0;

        var ip = _webHelper.GetCurrentIpAddress();
        var ua = Request.Headers.UserAgent.ToString();

        await _consentService.RecordConsentAsync(
            consentGuid,
            customerId,
            request.Functional,
            request.Analytics,
            request.Marketing,
            ip,
            ua);

        // Tarayıcıya consent cookie'si yaz
        WriteConsentCookie(consentGuid);

        return Ok(new SaveConsentResponse(
            Success: true,
            ConsentGuid: consentGuid.ToString("N"),
            PolicyVersion: TurkishConsumerLawDefaults.Cookies.CurrentPolicyVersion));
    }

    #endregion

    #region Helpers

    private Guid ReadOrCreateGuid()
    {
        if (Request.Cookies.TryGetValue(TurkishConsumerLawDefaults.Cookies.ConsentCookieName, out var raw)
            && Guid.TryParse(raw, out var existing))
            return existing;

        return Guid.NewGuid();
    }

    private void WriteConsentCookie(Guid guid)
    {
        Response.Cookies.Append(
            TurkishConsumerLawDefaults.Cookies.ConsentCookieName,
            guid.ToString("N"),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(TurkishConsumerLawDefaults.Cookies.ConsentCookieDays),
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true  // Necessary kategoride — consent gerekmez
            });
    }

    #endregion
}
