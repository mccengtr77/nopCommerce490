using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Controllers;

/// <summary>
/// KVKK ve ETK rıza kaydetme AJAX endpoint'leri.
/// Kayıt formundan veya müşteri profil sayfasından çağrılır.
///
/// Kayıt akışı:
/// 1. Müşteri formdaki checkbox'ları işaretler
/// 2. Form submit'inde POST → bu endpoint
/// 3. Her checkbox için ayrı <see cref="ConsentRecord"/> satırı oluşur
/// 4. Audit trail: IP/UA/SHA-256 hash
/// </summary>
[Route("Plugins/TurkishConsumerLaw/api/kvkk-consent")]
[ApiController]
public class KvkkConsentController : ControllerBase
{
    protected readonly IConsentRecordService _consentRecordService;
    protected readonly IExplicitConsentService _explicitConsentService;
    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly IWebHelper _webHelper;
    protected readonly ICustomerService _customerService;

    public KvkkConsentController(
        IConsentRecordService consentRecordService,
        IExplicitConsentService explicitConsentService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IWebHelper webHelper,
        ICustomerService customerService)
    {
        _consentRecordService = consentRecordService;
        _explicitConsentService = explicitConsentService;
        _workContext = workContext;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _customerService = customerService;
    }

    public record ConsentItemModel(int Scope, bool Granted);
    public record SaveConsentRequest(ConsentItemModel[] Consents, int Source = 1 /* Registration */);
    public record SaveConsentResponse(bool Success, int RecordsCreated);

    /// <summary>
    /// POST /Plugins/TurkishConsumerLaw/api/kvkk-consent
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Save([FromBody] SaveConsentRequest request)
    {
        if (request?.Consents is null || request.Consents.Length == 0)
            return BadRequest("Onay bilgisi yok.");

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer is null || !await _customerService.IsRegisteredAsync(customer))
            return Unauthorized("KVKK rıza kaydı için kayıtlı müşteri gerekir.");

        var store = await _storeContext.GetCurrentStoreAsync();
        var activeTexts = await _explicitConsentService.GetActiveAsync(store.Id);
        var versionByScope = activeTexts.ToDictionary(t => t.Scope, t => t.Version);

        var ip = _webHelper.GetCurrentIpAddress();
        var ua = Request.Headers.UserAgent.ToString();

        // Sadece tanımlı scope'lar kabul edilir; bilinmeyenler atlanır
        var batch = request.Consents
            .Where(c => Enum.IsDefined(typeof(ConsentScope), c.Scope))
            .Select(c =>
            {
                var scope = (ConsentScope)c.Scope;
                versionByScope.TryGetValue(scope, out var version);
                return (Scope: scope, Granted: c.Granted, TextVersion: version ?? "1.0");
            })
            .ToList();

        if (batch.Count == 0)
            return BadRequest("Geçerli scope yok.");

        var source = Enum.IsDefined(typeof(ConsentSource), request.Source)
            ? (ConsentSource)request.Source
            : ConsentSource.Registration;

        var records = await _consentRecordService.RecordBatchAsync(
            customer.Id, batch, source, ip, ua);

        return Ok(new SaveConsentResponse(Success: true, RecordsCreated: records.Count));
    }
}
