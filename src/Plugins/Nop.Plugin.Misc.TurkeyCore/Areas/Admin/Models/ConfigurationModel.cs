using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TurkeyCore.Areas.Admin.Models;

/// <summary>
/// TurkeyCore admin Configure sayfası için model.
/// nopCommerce convention: <see cref="BaseNopModel"/> + <see cref="NopResourceDisplayName"/>
/// ile lokalize label'lar.
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Misc.TurkeyCore.Settings.TcmbAutoUpdateEnabled")]
    public bool TcmbAutoUpdateEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkeyCore.Settings.GibMukellefServiceUrl")]
    public string GibMukellefServiceUrl { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkeyCore.Settings.GibCacheTimeMinutes")]
    public int GibCacheTimeMinutes { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkeyCore.Settings.TcknRequiredOnRegistration")]
    public bool TcknRequiredOnRegistration { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkeyCore.Settings.VknRequiredForCorporate")]
    public bool VknRequiredForCorporate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkeyCore.Settings.LocationSelectionRequired")]
    public bool LocationSelectionRequired { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkeyCore.Settings.DefaultVatRate")]
    public decimal DefaultVatRate { get; set; }

    /// <summary>
    /// Son TCMB güncellemesindeki kurlar (sayfa yüklenirken doldurulur, salt-okunur)
    /// </summary>
    public IList<ExchangeRateLog> LatestRates { get; set; } = new List<ExchangeRateLog>();

    /// <summary>
    /// 81 il + ilçe sayısı, mahalle sayısı, vergi dairesi sayısı — admin'e veri durumu özeti
    /// </summary>
    public DataStatusModel DataStatus { get; set; } = new();
}

public record DataStatusModel
{
    public int ProvinceCount { get; set; }
    public int DistrictCount { get; set; }
    public int NeighborhoodCount { get; set; }
    public int TaxOfficeCount { get; set; }
}
