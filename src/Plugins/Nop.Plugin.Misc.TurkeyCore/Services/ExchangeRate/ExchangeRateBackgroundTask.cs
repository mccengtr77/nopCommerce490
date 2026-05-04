using Nop.Plugin.Misc.TurkeyCore.Services.Product;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;

/// <summary>
/// Günlük TCMB döviz kuru güncelleme zamanlanmış görevi.
///
/// İki iş yapar:
/// 1. <see cref="ITcmbExchangeRateService.RefreshRatesAsync"/> — TCMB feed'i çek, ExchangeRateLog'a kaydet, cache invalide
/// 2. <see cref="ITurkishProductExtensionService.RecalculateAllAsync"/> — Tüm döviz bazlı ürün fiyatlarını
///    yeni kurla recalc edip <c>Product.Price/OldPrice/ProductCost</c> DB'ye yaz (persisted recalc).
///
/// nopCommerce admin panelinden Schedule Tasks altında görünür ve oradan tetiklenir.
/// Plugin install sırasında 24 saatlik aralıkla, disabled olarak kayıt edilir
/// (admin enable etmeden çalışmaz).
/// </summary>
public class ExchangeRateBackgroundTask : IScheduleTask
{
    #region Fields

    protected readonly ITcmbExchangeRateService _exchangeRateService;
    protected readonly ITurkishProductExtensionService _productExtensionService;
    protected readonly ILogger _logger;

    #endregion

    #region Ctor

    public ExchangeRateBackgroundTask(
        ITcmbExchangeRateService exchangeRateService,
        ITurkishProductExtensionService productExtensionService,
        ILogger logger)
    {
        _exchangeRateService = exchangeRateService;
        _productExtensionService = productExtensionService;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task ExecuteAsync()
    {
        var count = await _exchangeRateService.RefreshRatesAsync();
        await _logger.InformationAsync($"TCMB döviz kurları güncellendi: {count} kayıt eklendi");

        // Yeni kurla tüm döviz bazlı ürün fiyatlarını recalc edip DB'ye yaz.
        // Recalc kendi log'unu yazar; kur fetch fail olduysa cache eski olur ama recalc yine doğru çalışır.
        if (count > 0)
        {
            await _productExtensionService.RecalculateAllAsync();
        }
    }

    #endregion
}
