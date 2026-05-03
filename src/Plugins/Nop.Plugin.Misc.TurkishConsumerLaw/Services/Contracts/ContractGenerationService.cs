using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Services.Customer;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;

/// <inheritdoc />
public class ContractGenerationService : IContractGenerationService
{
    #region Fields

    protected readonly IRepository<GeneratedContract> _repository;
    protected readonly IContractTemplateService _templateService;
    protected readonly IContractTokenReplacer _tokenReplacer;
    protected readonly IOrderService _orderService;
    protected readonly ICustomerService _customerService;
    protected readonly IAddressService _addressService;
    protected readonly IProductService _productService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IStoreService _storeService;
    protected readonly ITurkishCustomerService _turkishCustomerService;

    #endregion

    public ContractGenerationService(
        IRepository<GeneratedContract> repository,
        IContractTemplateService templateService,
        IContractTokenReplacer tokenReplacer,
        IOrderService orderService,
        ICustomerService customerService,
        IAddressService addressService,
        IProductService productService,
        IPriceFormatter priceFormatter,
        IStoreService storeService,
        ITurkishCustomerService turkishCustomerService)
    {
        _repository = repository;
        _templateService = templateService;
        _tokenReplacer = tokenReplacer;
        _orderService = orderService;
        _customerService = customerService;
        _addressService = addressService;
        _productService = productService;
        _priceFormatter = priceFormatter;
        _storeService = storeService;
        _turkishCustomerService = turkishCustomerService;
    }

    /// <inheritdoc />
    public virtual async Task<IList<GeneratedContract>> GenerateForOrderAsync(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId)
            ?? throw new InvalidOperationException($"Sipariş {orderId} bulunamadı.");

        // İdempotency: zaten üretilmişse aynısını döndür
        var existing = await GetByOrderIdAsync(orderId);
        if (existing.Count > 0)
            return existing;

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        if (customer is null)
            throw new InvalidOperationException($"Müşteri {order.CustomerId} bulunamadı.");

        var context = await BuildContextAsync(order, customer);

        var generated = new List<GeneratedContract>();
        foreach (var type in new[] { ContractTemplateType.Mss, ContractTemplateType.Obf })
        {
            var template = await _templateService.GetActiveAsync(type, order.StoreId);
            if (template is null)
                continue; // Şablon yoksa atla (admin uyarılır, ama akış bloklanmaz)

            var html = _tokenReplacer.Replace(template.HtmlContent, context);

            var contract = new GeneratedContract
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                StoreId = order.StoreId,
                Type = type,
                TemplateId = template.Id,
                TemplateVersion = template.Version,
                HtmlContent = html,
                Accepted = false,
                ContentHash = ComputeHash(html),
                CreatedOnUtc = DateTime.UtcNow,
                ExpiresOnUtc = DateTime.UtcNow.AddYears(TurkishConsumerLawDefaults.Retention.ContractRetentionYears)
            };

            await _repository.InsertAsync(contract);
            generated.Add(contract);
        }

        return generated;
    }

    /// <inheritdoc />
    public virtual async Task<IList<GeneratedContract>> GetByOrderIdAsync(int orderId)
    {
        if (orderId <= 0)
            return new List<GeneratedContract>();

        return await _repository.GetAllAsync(
            query => query.Where(g => g.OrderId == orderId).OrderBy(g => g.Type),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task<GeneratedContract?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<IList<GeneratedContract>> GetByCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            return new List<GeneratedContract>();

        return await _repository.GetAllAsync(
            query => query.Where(g => g.CustomerId == customerId)
                          .OrderByDescending(g => g.CreatedOnUtc),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task MarkAcceptedAsync(int contractId, string ipAddress, string userAgent)
    {
        var contract = await GetByIdAsync(contractId)
            ?? throw new InvalidOperationException($"Sözleşme {contractId} bulunamadı.");

        if (contract.Accepted)
            return; // İdempotent

        contract.Accepted = true;
        contract.AcceptedOnUtc = DateTime.UtcNow;
        contract.AcceptanceIp = ipAddress ?? string.Empty;
        contract.AcceptanceUserAgent = userAgent ?? string.Empty;

        await _repository.UpdateAsync(contract);
    }

    #region Helpers

    /// <summary>
    /// Sipariş ve müşteri verilerinden token context oluşturur.
    /// </summary>
    protected virtual async Task<ContractTokenContext> BuildContextAsync(Order order, Customer customer)
    {
        var store = await _storeService.GetStoreByIdAsync(order.StoreId);
        var billingAddress = order.BillingAddressId > 0
            ? await _addressService.GetAddressByIdAsync(order.BillingAddressId)
            : null;
        var shippingAddress = order.ShippingAddressId.HasValue
            ? await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value)
            : null;

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        var productListHtml = await BuildProductListHtmlAsync(orderItems);

        // TurkeyCore ile TCKN/VKN
        var tckn = await _turkishCustomerService.GetTcKimlikNoAsync(customer);
        var vkn = await _turkishCustomerService.GetVergiNoAsync(customer);

        return new ContractTokenContext
        {
            // Satıcı (mağaza). Detaylı bilgi (KEP/MERSİS) admin Configure'dan ETBİS kaydı üzerinden
            // gelmeli — bu sürümde mağaza adı + URL kullanılıyor; ETBİS lookup eklenebilir.
            SellerName = store?.Name ?? string.Empty,
            SellerAddress = store?.CompanyAddress ?? string.Empty,
            SellerPhone = store?.CompanyPhoneNumber ?? string.Empty,
            SellerKep = string.Empty,    // TODO: ETBİS service entegrasyonu
            SellerMersis = string.Empty, // TODO: ETBİS service entegrasyonu

            BuyerName = $"{customer.FirstName} {customer.LastName}".Trim(),
            BuyerAddress = FormatAddress(billingAddress),
            BuyerPhone = billingAddress?.PhoneNumber ?? customer.Email ?? string.Empty,
            BuyerTcKimlikNo = tckn ?? string.Empty,
            BuyerVergiNo = vkn ?? string.Empty,

            OrderNumber = order.CustomOrderNumber ?? order.Id.ToString(),
            OrderDate = order.CreatedOnUtc.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("tr-TR")),
            ProductListHtml = productListHtml,

            Subtotal = await _priceFormatter.FormatPriceAsync(order.OrderSubtotalInclTax),
            TaxTotal = await _priceFormatter.FormatPriceAsync(order.OrderTax),
            ShippingFee = await _priceFormatter.FormatPriceAsync(order.OrderShippingInclTax),
            Discount = await _priceFormatter.FormatPriceAsync(order.OrderSubTotalDiscountInclTax),
            GrandTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal),

            ShippingAddress = FormatAddress(shippingAddress),
            BillingAddress = FormatAddress(billingAddress),

            PaymentMethod = order.PaymentMethodSystemName ?? string.Empty,
            ShippingMethod = order.ShippingMethod ?? string.Empty,
            EstimatedDeliveryTime = string.Empty, // TODO: shipment estimate

            WithdrawalDays = TurkishConsumerLawDefaults.Withdrawal.PeriodDays,
            MediationNotice = ContractTokenReplacer.DefaultMediationNotice
        };
    }

    /// <summary>
    /// Ürün listesini HTML tablo halinde üretir.
    /// </summary>
    protected virtual async Task<string> BuildProductListHtmlAsync(IList<OrderItem> items)
    {
        if (items is null || items.Count == 0)
            return "<p><em>Ürün bilgisi yok.</em></p>";

        var sb = new StringBuilder();
        sb.AppendLine("<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\" style=\"width:100%;border-collapse:collapse\">");
        sb.AppendLine("<thead><tr><th>Ürün</th><th>Adet</th><th>Birim Fiyat</th><th>Tutar</th></tr></thead>");
        sb.AppendLine("<tbody>");

        foreach (var item in items)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var name = product?.Name ?? $"Ürün #{item.ProductId}";
            var unit = await _priceFormatter.FormatPriceAsync(item.UnitPriceInclTax);
            var total = await _priceFormatter.FormatPriceAsync(item.PriceInclTax);

            sb.Append("<tr><td>")
              .Append(System.Net.WebUtility.HtmlEncode(name))
              .Append("</td><td>")
              .Append(item.Quantity)
              .Append("</td><td>")
              .Append(unit)
              .Append("</td><td>")
              .Append(total)
              .AppendLine("</td></tr>");
        }

        sb.AppendLine("</tbody></table>");
        return sb.ToString();
    }

    private static string FormatAddress(Nop.Core.Domain.Common.Address? address)
    {
        if (address is null) return string.Empty;
        var parts = new[]
        {
            address.Address1, address.Address2, address.City, address.ZipPostalCode, address.County
        }.Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    /// <summary>
    /// SHA-256 hash — sözleşme HTML içeriğinin manipülasyon kontrolü.
    /// </summary>
    public static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content ?? string.Empty));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    #endregion
}
