namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

/// <summary>
/// Onay (consent) kapsamları. Her bir kapsam için ayrı bir <see cref="ConsentRecord"/>
/// satırı oluşur — kullanıcı tek formla birden fazla kapsam için onay verirse,
/// her kapsam tek satır olur (audit trail).
///
/// 2026/347 İlke Kararı: "KVKK rızası ile ETK ticari ileti onayı AYRI olmalı".
/// Veri modelinde aynı tabloda saklanır, UI'da ayrı bölümlerde gösterilir.
/// Yasal ayırım UI'da; tablo yapısı sadece audit log.
/// </summary>
public enum ConsentScope
{
    // KVKK 6698 — açık rıza gerektiren işleme amaçları

    /// <summary>Kişisel verilerin işlenmesi — KVKK m.5/(2) açık rıza</summary>
    KvkkPersonalData = 1,

    /// <summary>Profil oluşturma ve segmentasyon — KVKK m.5</summary>
    KvkkProfiling = 2,

    /// <summary>Üçüncü taraflara aktarım (yurt içi)</summary>
    KvkkThirdPartyDomestic = 3,

    /// <summary>Yurt dışına aktarım — KVKK m.9 (en hassas)</summary>
    KvkkOverseasTransfer = 4,

    // ETK 6563 — ticari elektronik ileti onayları (AYRI dosyada saklanır, UI'da AYRI bölüm)

    /// <summary>SMS ile ticari ileti</summary>
    EtkSms = 10,

    /// <summary>E-posta ile ticari ileti</summary>
    EtkEmail = 11,

    /// <summary>Telefon araması ile ticari ileti</summary>
    EtkCall = 12
}
