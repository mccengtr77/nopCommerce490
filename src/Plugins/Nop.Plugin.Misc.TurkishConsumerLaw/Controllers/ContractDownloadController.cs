using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Controllers;

/// <summary>
/// Müşteri kendi sözleşmelerini HTML olarak indirebilir.
/// PDF'e dönüştürme (QuestPDF) sonraki sürümde — şimdilik HTML.
/// </summary>
[Route("Plugins/TurkishConsumerLaw/contracts")]
public class ContractDownloadController : Controller
{
    protected readonly IContractGenerationService _service;
    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;

    public ContractDownloadController(
        IContractGenerationService service,
        IWorkContext workContext,
        ICustomerService customerService)
    {
        _service = service;
        _workContext = workContext;
        _customerService = customerService;
    }

    /// <summary>
    /// GET /Plugins/TurkishConsumerLaw/contracts/download/{id}
    /// </summary>
    [HttpGet("download/{id:int}")]
    public async Task<IActionResult> Download(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer is null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var contract = await _service.GetByIdAsync(id);
        if (contract is null || contract.CustomerId != customer.Id)
            return NotFound();

        // HTML döner — PDF için ileride QuestPDF veya PdfSharpCore eklenir
        var html = WrapWithDocumentShell(contract.HtmlContent, contract.Type.ToString(), contract.OrderId);
        var bytes = Encoding.UTF8.GetBytes(html);
        var fileName = $"{contract.Type}_{contract.OrderId}_{contract.Id}.html";

        return File(bytes, "text/html; charset=utf-8", fileName);
    }

    private static string WrapWithDocumentShell(string innerHtml, string typeName, int orderId)
    {
        // $$ ile çift-dolar raw string: { literal, {{var}} interpolation
        return $$"""
            <!DOCTYPE html>
            <html lang="tr">
            <head>
              <meta charset="utf-8" />
              <title>{{typeName}} - Sipariş #{{orderId}}</title>
              <style>
                body { font-family: 'Segoe UI', Arial, sans-serif; max-width: 760px; margin: 24px auto; padding: 16px; line-height: 1.5; }
                h2, h3, h4 { margin-top: 1.2em; }
                table { width: 100%; border-collapse: collapse; }
                th, td { border: 1px solid #ccc; padding: 6px; text-align: left; }
                th { background: #f5f5f5; }
              </style>
            </head>
            <body>
              {{innerHtml}}
            </body>
            </html>
            """;
    }
}
