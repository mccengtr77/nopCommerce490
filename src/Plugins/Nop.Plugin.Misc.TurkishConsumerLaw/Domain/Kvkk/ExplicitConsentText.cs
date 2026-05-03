using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

/// <summary>
/// Açık rıza metni şablonu — her <see cref="ConsentScope"/> için ayrı metin tutulabilir.
/// 2026/347 İlke Kararı: Aydınlatma metninden AYRI dökümandır; tek kutucuk altında
/// birden fazla rıza birleştirilemez.
/// </summary>
public class ExplicitConsentText : BaseEntity
{
    /// <summary>
    /// Hangi kapsam için olduğu
    /// </summary>
    public ConsentScope Scope { get; set; }

    /// <summary>
    /// UI'da checkbox yanında gösterilecek kısa label
    /// </summary>
    public string ShortLabel { get; set; } = string.Empty;

    /// <summary>
    /// Detaylı metin (HTML, modal/expand'da gösterilir)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Sürüm — değişince eski rıza "stale" sayılır
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// 0 = tüm mağazalar
    /// </summary>
    public int LimitedToStoreId { get; set; }

    /// <summary>
    /// Default işaretli mi (varsayılan: false — açık rıza opt-in olmalı; true sadece zorunlu kapsamlar için)
    /// </summary>
    public bool DefaultChecked { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}
