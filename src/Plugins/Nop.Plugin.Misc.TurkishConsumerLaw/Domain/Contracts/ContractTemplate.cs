using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

/// <summary>
/// Sözleşme şablonu. Admin tarafından düzenlenir, OrderPlaced anında token replace edilerek
/// kullanıcıya özelleştirilmiş <see cref="GeneratedContract"/> üretilir.
///
/// Versiyonlama: Şablon değişince <see cref="Version"/> artırılır; eski versiyonlu üretilmiş
/// sözleşmeler audit için saklanır (yasal denetim).
/// </summary>
public class ContractTemplate : BaseEntity
{
    /// <summary>MSS / ÖBF</summary>
    public ContractTemplateType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Token'lı HTML içerik</summary>
    public string HtmlContent { get; set; } = string.Empty;

    public string Version { get; set; } = "1.0";

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    /// <summary>0 = tüm mağazalar</summary>
    public int LimitedToStoreId { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}
