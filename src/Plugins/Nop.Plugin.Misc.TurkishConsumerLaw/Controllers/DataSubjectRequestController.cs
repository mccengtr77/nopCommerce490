using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.DataSubjectRequests;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Controllers;

/// <summary>
/// KVKK m.11 veri sahibi başvurusu storefront endpoint'i.
///
/// Anonim ziyaretçiler de başvuru yapabilir (KVKK kişiyi koruyor — kayıt zorunluluğu yok).
/// E-posta ile yanıtlanır. Kayıtlı müşteri ise CustomerId set edilir.
/// </summary>
[Route("Plugins/TurkishConsumerLaw/api/data-subject-request")]
[ApiController]
public class DataSubjectRequestController : ControllerBase
{
    protected readonly IDataSubjectRequestService _service;
    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly IWebHelper _webHelper;
    protected readonly ICustomerService _customerService;

    public DataSubjectRequestController(
        IDataSubjectRequestService service,
        IWorkContext workContext,
        IStoreContext storeContext,
        IWebHelper webHelper,
        ICustomerService customerService)
    {
        _service = service;
        _workContext = workContext;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _customerService = customerService;
    }

    public record SubmitRequest(
        string FullName,
        string Email,
        string? Phone,
        int RequestType,
        string Description);

    public record SubmitResponse(bool Success, int RequestId, DateTime DeadlineUtc, string? Error = null);

    /// <summary>
    /// POST /Plugins/TurkishConsumerLaw/api/data-subject-request
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Submit([FromBody] SubmitRequest req)
    {
        if (req is null)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(req.FullName) || req.FullName.Trim().Length < 2)
            return Ok(new SubmitResponse(false, 0, default, "Ad soyad zorunludur."));

        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
            return Ok(new SubmitResponse(false, 0, default, "Geçerli bir e-posta adresi girin."));

        if (!Enum.IsDefined(typeof(DataSubjectRequestType), req.RequestType))
            return Ok(new SubmitResponse(false, 0, default, "Geçersiz başvuru türü."));

        if (string.IsNullOrWhiteSpace(req.Description) || req.Description.Trim().Length < 10)
            return Ok(new SubmitResponse(false, 0, default, "Talep açıklaması en az 10 karakter olmalıdır."));

        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerId = customer is not null && await _customerService.IsRegisteredAsync(customer)
            ? customer.Id
            : 0;

        var store = await _storeContext.GetCurrentStoreAsync();

        var request = new DataSubjectRequest
        {
            CustomerId = customerId,
            FullName = req.FullName.Trim(),
            Email = req.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim(),
            RequestType = (DataSubjectRequestType)req.RequestType,
            Description = req.Description.Trim(),
            IpAddress = _webHelper.GetCurrentIpAddress() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString(),
            StoreId = store.Id
        };

        var saved = await _service.SubmitAsync(request);

        return Ok(new SubmitResponse(true, saved.Id, saved.DeadlineUtc));
    }
}
