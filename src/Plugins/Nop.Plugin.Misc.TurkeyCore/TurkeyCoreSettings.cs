using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.TurkeyCore;

/// <summary>
/// TurkeyCore plugin ayarları — multi-store destekli
/// </summary>
public class TurkeyCoreSettings : ISettings
{
    /// <summary>
    /// TCMB döviz kuru otomatik güncelleme aktif mi
    /// </summary>
    public bool TcmbAutoUpdateEnabled { get; set; } = true;

    /// <summary>
    /// GİB mükellef sorgu endpoint URL
    /// </summary>
    public string GibMukellefServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// GİB mükellef sorgu cache süresi (dakika)
    /// </summary>
    public int GibCacheTimeMinutes { get; set; } = 1440;

    /// <summary>
    /// Kayıt formunda TCKN zorunlu mu
    /// </summary>
    public bool TcknRequiredOnRegistration { get; set; }

    /// <summary>
    /// Kurumsal müşteriler için VKN zorunlu mu
    /// </summary>
    public bool VknRequiredForCorporate { get; set; } = true;

    /// <summary>
    /// Checkout'ta il/ilçe/mahalle seçimi zorunlu mu
    /// </summary>
    public bool LocationSelectionRequired { get; set; } = true;

    /// <summary>
    /// Varsayılan KDV oranı (%)
    /// </summary>
    public decimal DefaultVatRate { get; set; } = 20m;
}
