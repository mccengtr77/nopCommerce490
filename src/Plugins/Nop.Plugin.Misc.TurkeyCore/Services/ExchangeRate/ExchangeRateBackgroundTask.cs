using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;

/// <summary>
/// Günlük TCMB döviz kuru güncelleme zamanlanmış görevi.
/// nopCommerce admin panelinden Schedule Tasks altında görünür ve oradan tetiklenir.
/// Plugin install sırasında 24 saatlik aralıkla, disabled olarak kayıt edilir
/// (admin enable etmeden çalışmaz).
/// </summary>
public class ExchangeRateBackgroundTask : IScheduleTask
{
    #region Fields

    protected readonly ITcmbExchangeRateService _exchangeRateService;
    protected readonly ILogger _logger;

    #endregion

    #region Ctor

    public ExchangeRateBackgroundTask(
        ITcmbExchangeRateService exchangeRateService,
        ILogger logger)
    {
        _exchangeRateService = exchangeRateService;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task ExecuteAsync()
    {
        var count = await _exchangeRateService.RefreshRatesAsync();
        await _logger.InformationAsync($"TCMB döviz kurları güncellendi: {count} kayıt eklendi");
    }

    #endregion
}
