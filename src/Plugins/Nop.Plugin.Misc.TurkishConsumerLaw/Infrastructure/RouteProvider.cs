using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: TurkishConsumerLawDefaults.ConfigurationRouteName,
            pattern: "Admin/TurkishConsumerLawSettings/Configure",
            defaults: new { controller = "TurkishConsumerLawSettings", action = "Configure", area = "Admin" });
    }

    public int Priority => 0;
}
