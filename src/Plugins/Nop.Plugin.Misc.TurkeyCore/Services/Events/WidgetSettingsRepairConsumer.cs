using Nop.Core.Domain.Cms;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Events;

/// <summary>
/// Defensive self-healing consumer.
///
/// Plugin'in <see cref="WidgetSettings.ActiveWidgetSystemNames"/> listesine system name'i eklenmiş
/// olmalı (aksi halde widget zone'ları çalışmaz). Normalde bu <see cref="TurkeyCorePlugin.InstallAsync"/>
/// veya <see cref="TurkeyCorePlugin.UpdateAsync"/> tarafından eklenir, ancak plugin lifecycle
/// hook'ları (özellikle update path'i sessizce fail edebilir) her zaman güvenilir değil.
///
/// Bu consumer her app startup'ta çalışır ve plugin systemName listede yoksa ekler.
/// Yan etkisiz: zaten varsa no-op.
/// </summary>
public class WidgetSettingsRepairConsumer : IConsumer<AppStartedEvent>
{
    private readonly WidgetSettings _widgetSettings;
    private readonly ISettingService _settingService;
    private readonly ILogger _logger;

    public WidgetSettingsRepairConsumer(
        WidgetSettings widgetSettings,
        ISettingService settingService,
        ILogger logger)
    {
        _widgetSettings = widgetSettings;
        _settingService = settingService;
        _logger = logger;
    }

    public async Task HandleEventAsync(AppStartedEvent eventMessage)
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        if (_widgetSettings.ActiveWidgetSystemNames.Contains(TurkeyCoreDefaults.SystemName))
            return;

        try
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(TurkeyCoreDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
            await _logger.InformationAsync(
                $"TurkeyCore: WidgetSettings.ActiveWidgetSystemNames listesine '{TurkeyCoreDefaults.SystemName}' eklendi (self-healing).");
        }
        catch (Exception ex)
        {
            await _logger.WarningAsync(
                $"TurkeyCore: WidgetSettings repair başarısız: {ex.Message}", ex);
        }
    }
}
