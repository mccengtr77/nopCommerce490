using Nop.Core.Caching;

namespace Nop.Plugin.Misc.TurkishConsumerLaw;

/// <summary>
/// TurkishConsumerLaw plugin sabitleri.
///
/// Yasal çerçeve referansı:
/// - 6502 sayılı Tüketicinin Korunması Hakkında Kanun + Mesafeli Sözleşmeler Yönetmeliği (yürürlük 01.01.2026)
/// - 6698 sayılı KVKK + 24.03.2026 tarihli 2026/347 İlke Kararı (aydınlatma/rıza ayrımı)
/// - 6563 sayılı E-Ticaret Kanunu + ETBİS yönetmeliği
/// - VUK 509/535 nolu Tebliğ (e-fatura/e-arşiv)
/// </summary>
public static class TurkishConsumerLawDefaults
{
    public const string SystemName = "Misc.TurkishConsumerLaw";

    public const string ConfigurationRouteName = "Plugin.Misc.TurkishConsumerLaw.Configure";

    public const string LocaleStringResourcesPrefix = "Plugins.Misc.TurkishConsumerLaw";

    /// <summary>
    /// CacheKey tanımları
    /// </summary>
    public static class Cache
    {
        /// <summary>Aktif ETBİS kaydı (parametresiz, mağaza başına 1 olur)</summary>
        public static CacheKey ActiveEtbis => new("Nop.Plugin.TurkishConsumerLaw.Etbis.Active");

        /// <summary>Aktif çerez tanımları (kategori bazlı listeleme için)</summary>
        public static CacheKey ActiveCookieDefinitions => new("Nop.Plugin.TurkishConsumerLaw.Cookies.Definitions.Active");

        /// <summary>Aktif KVKK aydınlatma metni — {0} = StoreId</summary>
        public static CacheKey ActiveDisclosureText => new("Nop.Plugin.TurkishConsumerLaw.Kvkk.DisclosureText.{0}");

        /// <summary>Aktif açık rıza metinleri — {0} = StoreId</summary>
        public static CacheKey ActiveExplicitConsentTexts => new("Nop.Plugin.TurkishConsumerLaw.Kvkk.ExplicitConsents.{0}");
    }

    /// <summary>
    /// Çerez consent storage sabitleri.
    /// </summary>
    public static class Cookies
    {
        /// <summary>
        /// Tarayıcıya yazılan consent cookie'sinin adı.
        /// Bu cookie kendi başına "Necessary" kategoride — onay olmadan da çalışır.
        /// </summary>
        public const string ConsentCookieName = "tcl-consent";

        /// <summary>Consent cookie geçerlilik süresi (gün) — KVKK önerisi 6 ay</summary>
        public const int ConsentCookieDays = 180;

        /// <summary>Mevcut çerez politikası versiyonu — değişince eski rıza geçersiz</summary>
        public const string CurrentPolicyVersion = "1.0";
    }

    /// <summary>
    /// MSS/ÖBF saklama sabitleri — 6502 sayılı kanun saklama yükümlülüğü
    /// </summary>
    public static class Retention
    {
        /// <summary>
        /// Mesafeli sözleşme saklama süresi — Tüketici Kanunu m.16/(1) "iade ve garanti süreleri" + audit
        /// </summary>
        public const int ContractRetentionYears = 3;

        /// <summary>
        /// KVKK rıza kayıtları — KVKK m.7 saklama süresi sonu sonrası imha zorunlu
        /// Default 10 yıl; sektörel düzenlemeler farklı olabilir
        /// </summary>
        public const int ConsentRetentionYears = 10;
    }

    /// <summary>
    /// Cayma hakkı sabitleri — Mesafeli Sözleşmeler Yönetmeliği m.9
    /// </summary>
    public static class Withdrawal
    {
        /// <summary>Cayma hakkı süresi (gün) — teslimattan itibaren</summary>
        public const int PeriodDays = 14;

        /// <summary>
        /// 2026 güncellemesi: İade kargo ücretini SATICI öder.
        /// Yönetmelik m.13/(2) — değişiklik 24.05.2025/32557
        /// </summary>
        public const bool ReturnShippingPaidBySeller = true;
    }

    /// <summary>
    /// İYS (İleti Yönetim Sistemi) sabitleri — 6563 sayılı kanun
    /// </summary>
    public static class Iys
    {
        /// <summary>Onay alındıktan sonra İYS'ye upload SLA — 3 iş günü</summary>
        public const int UploadSlaDays = 3;

        public const string ScheduleTaskType = "Nop.Plugin.Misc.TurkishConsumerLaw.Services.Iys.IysSyncBackgroundTask, Nop.Plugin.Misc.TurkishConsumerLaw";
        public const string ScheduleTaskName = "İYS Sync (3 günlük SLA)";
        public const int DefaultScheduleIntervalSeconds = 21600; // 6 saat
    }

    /// <summary>
    /// ETBİS sabitleri — 6563 sayılı E-Ticaret Kanunu, ETBİS Yönetmeliği
    /// </summary>
    public static class Etbis
    {
        /// <summary>
        /// ETBİS doğrulama URL'i — eticaret.gov.tr üzerinden mağaza arama
        /// </summary>
        public const string VerificationUrlPrefix = "https://eticaret.gov.tr";
    }
}
