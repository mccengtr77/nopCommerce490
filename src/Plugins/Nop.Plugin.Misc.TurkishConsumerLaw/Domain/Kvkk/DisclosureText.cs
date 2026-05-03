using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

/// <summary>
/// KVKK aydınlatma metni şablonu. KVKK m.10 ve Aydınlatma Yükümlülüğü Tebliği gereği
/// veri sorumlusu, müşteriye işleme amaçlarını, hakları, başvuru yollarını AYDINLATMA
/// metniyle bildirmek zorundadır.
///
/// 2026/347 İlke Kararı: Aydınlatma metni içinde "açık rıza" ifadesi YER ALMAZ —
/// ayrı dökümandır. Bu metnin sonuna sadece "Okudum ve anladım" beyanı konabilir.
/// </summary>
public class DisclosureText : BaseEntity
{
    /// <summary>
    /// Başlık (admin için)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// HTML içerik (TinyMCE editörle düzenlenir)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Metin versiyonu — değiştiğinde tüm aktif onaylar "stale" sayılır,
    /// kullanıcıdan tekrar "Okudum" beyanı istenir.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// 0 = tüm mağazalar
    /// </summary>
    public int LimitedToStoreId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}
