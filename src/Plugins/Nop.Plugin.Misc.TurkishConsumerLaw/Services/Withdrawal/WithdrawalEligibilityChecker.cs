using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Orders;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;

/// <inheritdoc />
public class WithdrawalEligibilityChecker : IWithdrawalEligibilityChecker
{
    protected readonly IOrderService _orderService;
    protected readonly IShipmentService _shipmentService;

    public WithdrawalEligibilityChecker(
        IOrderService orderService,
        IShipmentService shipmentService)
    {
        _orderService = orderService;
        _shipmentService = shipmentService;
    }

    /// <inheritdoc />
    public virtual async Task<WithdrawalEligibilityResult> CheckAsync(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order is null || order.Deleted)
            return new(false, "Sipariş bulunamadı.", orderId, null, null);

        // Müşteri ödeme tamamlamadıysa cayma değil cancel akışı uygulanmalı
        if (order.PaymentStatus != PaymentStatus.Paid)
            return new(false,
                "Sipariş henüz ödenmemiş. Bu durum için cayma değil iptal hakkını kullanabilirsiniz.",
                orderId, null, null);

        // İade edilmiş veya kısmen iade edilmiş — yine cayma yapılabilir mi? Genelde hayır.
        if (order.PaymentStatus is PaymentStatus.Refunded or PaymentStatus.PartiallyRefunded)
            return new(false,
                "Sipariş zaten iade edilmiş.",
                orderId, null, null);

        // Sayaç başlangıcı: en güncel teslim edilmiş shipment'ın delivery tarihi.
        // Yoksa shipping zorunlu olmayan sipariş için OrderCreatedOnUtc.
        var startUtc = await GetWithdrawalStartDateAsync(order);
        var deadlineUtc = startUtc.AddDays(TurkishConsumerLawDefaults.Withdrawal.PeriodDays);
        var daysRemaining = (deadlineUtc - DateTime.UtcNow).Days;

        if (DateTime.UtcNow > deadlineUtc)
            return new(false,
                $"14 günlük cayma süresi {Math.Abs(daysRemaining)} gün önce sona erdi.",
                orderId, deadlineUtc, daysRemaining);

        return new(true, null, orderId, deadlineUtc, daysRemaining);
    }

    /// <summary>
    /// Cayma süresi başlangıç tarihi:
    /// 1. En son teslim edilmiş shipment.DeliveryDateUtc varsa o
    /// 2. Yoksa Order.CreatedOnUtc (digital ürünler veya shipment yapılmamış sipariş için)
    /// </summary>
    protected virtual async Task<DateTime> GetWithdrawalStartDateAsync(Order order)
    {
        var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(order.Id);
        var latestDelivery = shipments
            .Where(s => s.DeliveryDateUtc.HasValue)
            .Select(s => s.DeliveryDateUtc!.Value)
            .DefaultIfEmpty(default)
            .Max();

        return latestDelivery == default ? order.CreatedOnUtc : latestDelivery;
    }
}
