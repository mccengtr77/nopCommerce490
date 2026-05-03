using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Controllers;

/// <summary>
/// Müşteri cayma talebi storefront endpoint'i.
///
/// Flow:
/// 1. Müşteri panelinde "Sipariş Iade Et" → eligibility kontrol (GET /eligibility/{orderId})
/// 2. Eligible ise form göster, submit → POST
/// 3. Admin onayını bekle
/// </summary>
[Route("Plugins/TurkishConsumerLaw/api/withdrawal")]
[ApiController]
public class WithdrawalController : ControllerBase
{
    protected readonly IWithdrawalService _service;
    protected readonly IWithdrawalEligibilityChecker _eligibilityChecker;
    protected readonly IOrderService _orderService;
    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly IWebHelper _webHelper;
    protected readonly ICustomerService _customerService;

    public WithdrawalController(
        IWithdrawalService service,
        IWithdrawalEligibilityChecker eligibilityChecker,
        IOrderService orderService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IWebHelper webHelper,
        ICustomerService customerService)
    {
        _service = service;
        _eligibilityChecker = eligibilityChecker;
        _orderService = orderService;
        _workContext = workContext;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _customerService = customerService;
    }

    public record EligibilityResponse(bool IsEligible, string? Reason, DateTime? DeadlineUtc, int? DaysRemaining, bool AlreadyRequested);

    public record SubmitRequest(int OrderId, int Reason, string Description);
    public record SubmitResponse(bool Success, int RequestId, string? Error = null);

    /// <summary>
    /// GET /Plugins/TurkishConsumerLaw/api/withdrawal/eligibility/{orderId}
    /// </summary>
    [HttpGet("eligibility/{orderId:int}")]
    public async Task<IActionResult> Eligibility(int orderId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer is null || !await _customerService.IsRegisteredAsync(customer))
            return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order is null || order.CustomerId != customer.Id)
            return NotFound();

        var existing = await _service.GetByOrderIdAsync(orderId);

        // Daha önce talep edilip henüz kapatılmamışsa yeni talep alınmaz
        if (existing is not null && existing.Status is not (WithdrawalStatus.Rejected or WithdrawalStatus.Cancelled))
        {
            return Ok(new EligibilityResponse(
                IsEligible: false,
                Reason: $"Bu sipariş için zaten cayma talebiniz var (#{existing.Id} — {existing.Status}).",
                DeadlineUtc: null,
                DaysRemaining: null,
                AlreadyRequested: true));
        }

        var result = await _eligibilityChecker.CheckAsync(orderId);

        return Ok(new EligibilityResponse(
            IsEligible: result.IsEligible,
            Reason: result.Reason,
            DeadlineUtc: result.DeadlineUtc,
            DaysRemaining: result.DaysRemaining,
            AlreadyRequested: false));
    }

    /// <summary>
    /// POST /Plugins/TurkishConsumerLaw/api/withdrawal
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Submit([FromBody] SubmitRequest req)
    {
        if (req is null || req.OrderId <= 0)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(req.Description) || req.Description.Trim().Length < 10)
            return Ok(new SubmitResponse(false, 0, "Açıklama en az 10 karakter olmalıdır."));

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer is null || !await _customerService.IsRegisteredAsync(customer))
            return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(req.OrderId);
        if (order is null || order.CustomerId != customer.Id)
            return NotFound();

        // Eligibility tekrar kontrol — TOCTTOU önlemi
        var eligibility = await _eligibilityChecker.CheckAsync(req.OrderId);
        if (!eligibility.IsEligible)
            return Ok(new SubmitResponse(false, 0, eligibility.Reason));

        var existing = await _service.GetByOrderIdAsync(req.OrderId);
        if (existing is not null && existing.Status is not (WithdrawalStatus.Rejected or WithdrawalStatus.Cancelled))
            return Ok(new SubmitResponse(false, existing.Id,
                "Bu sipariş için zaten aktif bir cayma talebiniz var."));

        var reason = Enum.IsDefined(typeof(WithdrawalReason), req.Reason)
            ? (WithdrawalReason)req.Reason
            : WithdrawalReason.NotSpecified;

        var store = await _storeContext.GetCurrentStoreAsync();

        var request = new WithdrawalRequest
        {
            OrderId = req.OrderId,
            CustomerId = customer.Id,
            Reason = reason,
            Description = req.Description.Trim(),
            IpAddress = _webHelper.GetCurrentIpAddress() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString(),
            StoreId = store.Id
        };

        var saved = await _service.SubmitAsync(request);
        return Ok(new SubmitResponse(true, saved.Id));
    }
}
