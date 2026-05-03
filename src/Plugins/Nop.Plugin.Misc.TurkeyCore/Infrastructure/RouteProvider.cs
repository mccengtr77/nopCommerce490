using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.TurkeyCore.Infrastructure;

/// <summary>
/// TurkeyCore plugin route provider.
/// API endpoint'leri attribute routing ile <see cref="Controllers.TurkeyCoreApiController"/>
/// üzerinde tanımlı; bu provider sadece named admin route'larını ve özel kısa yolları kayıt eder.
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <inheritdoc />
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Admin Configure sayfası — plugin store'da "Configure" linki için named route
        endpointRouteBuilder.MapControllerRoute(
            name: TurkeyCoreDefaults.ConfigurationRouteName,
            pattern: "Admin/TurkeyCoreSettings/Configure",
            defaults: new { controller = "TurkeyCoreSettings", action = "Configure", area = "Admin" });
    }

    /// <summary>
    /// Standart plugin route'u — nopCommerce default'larından önce çalışsın diye 0
    /// </summary>
    public int Priority => 0;
}
