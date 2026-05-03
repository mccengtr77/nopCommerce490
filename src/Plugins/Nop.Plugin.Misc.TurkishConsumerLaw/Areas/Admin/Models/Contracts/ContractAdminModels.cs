using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models.Contracts;

public record ContractTemplateListModel : BaseNopModel
{
    public IList<ContractTemplateRowModel> Items { get; set; } = new List<ContractTemplateRowModel>();
}

public record ContractTemplateRowModel : BaseNopEntityModel
{
    public ContractTemplateType Type { get; set; }
    public string TypeLabel { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}

public record ContractTemplateEditModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Contracts.Type")]
    public ContractTemplateType Type { get; set; } = ContractTemplateType.Mss;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Contracts.Name")]
    public string Name { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Contracts.HtmlContent")]
    public string HtmlContent { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Contracts.Version")]
    public string Version { get; set; } = "1.0";

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Contracts.IsActive")]
    public bool IsActive { get; set; } = true;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Contracts.DisplayOrder")]
    public int DisplayOrder { get; set; }
}

public record GeneratedContractListModel : BaseNopModel
{
    public IList<GeneratedContractRowModel> Items { get; set; } = new List<GeneratedContractRowModel>();
}

public record GeneratedContractRowModel : BaseNopEntityModel
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public ContractTemplateType Type { get; set; }
    public string TypeLabel { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public bool Accepted { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? AcceptedOnUtc { get; set; }
}
