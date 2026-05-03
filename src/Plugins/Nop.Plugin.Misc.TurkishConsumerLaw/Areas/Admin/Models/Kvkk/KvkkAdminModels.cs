using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.Kvkk;

/// <summary>
/// KVKK aydınlatma + açık rıza metinlerini tek sayfada listeler.
/// </summary>
public record KvkkTextsListModel : BaseNopModel
{
    /// <summary>
    /// Admin sadece tek aktif aydınlatma metni tutuyor (multi-store fallback'li).
    /// </summary>
    public DisclosureTextModel? Disclosure { get; set; }

    public IList<ExplicitConsentRowModel> ExplicitConsents { get; set; } = new List<ExplicitConsentRowModel>();
}

public record DisclosureTextModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.Title")]
    public string Title { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.Content")]
    public string Content { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.Version")]
    public string Version { get; set; } = "1.0";

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.IsActive")]
    public bool IsActive { get; set; } = true;
}

public record ExplicitConsentRowModel : BaseNopEntityModel
{
    public ConsentScope Scope { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;
    public string ShortLabel { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool DefaultChecked { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public record ExplicitConsentEditModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.Scope")]
    public ConsentScope Scope { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.ShortLabel")]
    public string ShortLabel { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.Content")]
    public string Content { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.Version")]
    public string Version { get; set; } = "1.0";

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.DefaultChecked")]
    public bool DefaultChecked { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Kvkk.IsActive")]
    public bool IsActive { get; set; } = true;
}
