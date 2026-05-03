using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Events;

/// <summary>
/// Sipariş tamamlandığında MSS ve ÖBF otomatik üretilir ve veritabanına kaydedilir.
/// Müşteri "okudum kabul ediyorum" işaretini sipariş onayında verdiyse bu sözleşmeler
/// <see cref="ContractGenerationService"/> üzerinden ayrıca <c>Accepted</c> olarak işaretlenir.
///
/// İdempotent: aynı sipariş için zaten üretilmiş varsa tekrar üretilmez.
/// </summary>
public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    protected readonly IContractGenerationService _generationService;
    protected readonly ILogger _logger;

    public OrderPlacedConsumer(
        IContractGenerationService generationService,
        ILogger logger)
    {
        _generationService = generationService;
        _logger = logger;
    }

    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        var order = eventMessage?.Order;
        if (order is null || order.Id <= 0)
            return;

        try
        {
            await _generationService.GenerateForOrderAsync(order.Id);
        }
        catch (Exception ex)
        {
            // Sipariş akışını bloklamamak için yutuyoruz; admin log'undan görebilir.
            // Kritik bir yasal süreç olduğu için tekrar üretim için admin manuel müdahale yapabilir
            // (ileride: retry queue eklenebilir).
            await _logger.ErrorAsync(
                $"Sözleşme üretimi başarısız (OrderId={order.Id}): {ex.Message}", ex);
        }
    }
}
