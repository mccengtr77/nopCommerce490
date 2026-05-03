using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Misc.TurkeyCore.Infrastructure;

/// <summary>
/// Plugin'in shared view'larının (ör. <c>_CreateOrUpdateAddress.cshtml</c> override'ı)
/// Razor view engine tarafından bulunabilmesi için ek path tanımlar.
///
/// nopCommerce default'unda sadece <c>Themes/{theme}/Views/Shared/</c> ve nopCommerce'in
/// kendi <c>Views/Shared/</c> klasörü taranıyor. Plugin override'larını da search path'e
/// koymak için bu expander'ı NopStartup'tan kaydederiz.
///
/// Tema override'ı plugin override'ından önce gelir (theme expander daha önce eklendi),
/// dolayısıyla aktif tema (Pavilion vb.) plugin override'ını ezebilir — istenen davranış.
/// </summary>
public class PluginViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        // Sabit path; cache key'e ek bilgi gerekmiyor.
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        var pluginPath = $"/Plugins/{TurkeyCoreDefaults.SystemName}/Views/{{1}}/{{0}}.cshtml";
        var pluginShared = $"/Plugins/{TurkeyCoreDefaults.SystemName}/Views/Shared/{{0}}.cshtml";

        // Plugin path'lerini default path'lerden önce dene; tema path'leri zaten daha
        // önce ThemeableViewLocationExpander tarafından eklendiği için onlar bizden de önce gelir.
        return new[] { pluginPath, pluginShared }.Concat(viewLocations);
    }
}
