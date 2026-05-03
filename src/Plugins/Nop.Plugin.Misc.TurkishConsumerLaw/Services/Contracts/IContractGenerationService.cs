using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;

/// <summary>
/// Sipariş için MSS ve ÖBF üretim servisi. Aktif şablonlardan token replace ederek
/// <see cref="GeneratedContract"/> kayıtları oluşturur.
/// </summary>
public interface IContractGenerationService
{
    /// <summary>
    /// Sipariş için MSS ve ÖBF üretip kaydeder. Sipariş için zaten üretilmiş varsa tekrar üretmez.
    /// </summary>
    Task<IList<GeneratedContract>> GenerateForOrderAsync(int orderId);

    /// <summary>
    /// Sipariş için üretilen sözleşmeleri döner (varsa).
    /// </summary>
    Task<IList<GeneratedContract>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Belirli sözleşmeyi Id ile getirir.
    /// </summary>
    Task<GeneratedContract?> GetByIdAsync(int id);

    /// <summary>
    /// Müşterinin tüm sözleşmelerini döner (müşteri panelinde "Sözleşmelerim").
    /// </summary>
    Task<IList<GeneratedContract>> GetByCustomerAsync(int customerId);

    /// <summary>
    /// Checkout'ta müşteri "okudum kabul ediyorum" işaretlediğinde çağrılır.
    /// </summary>
    Task MarkAcceptedAsync(int contractId, string ipAddress, string userAgent);
}
