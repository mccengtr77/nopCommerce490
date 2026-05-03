using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.Cookies;

public record CookieDefinitionListModel : BaseNopModel
{
    public IList<CookieDefinitionRowModel> Items { get; set; } = new List<CookieDefinitionRowModel>();
}

public record CookieDefinitionRowModel : BaseNopEntityModel
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public CookieCategory Category { get; set; }
    public string CategoryLabel { get; set; } = string.Empty;
    public bool IsThirdParty { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public record CookieDefinitionEditModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Cookies.Name")]
    public string Name { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Cookies.Provider")]
    public string Provider { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Cookies.Purpose")]
    public string Purpose { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Cookies.Duration")]
    public string Duration { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Cookies.Category")]
    public CookieCategory Category { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Cookies.IsThirdParty")]
    public bool IsThirdParty { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Cookies.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Cookies.IsActive")]
    public bool IsActive { get; set; } = true;
}
