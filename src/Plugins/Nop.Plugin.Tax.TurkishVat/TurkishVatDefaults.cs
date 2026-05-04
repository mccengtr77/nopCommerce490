namespace Nop.Plugin.Tax.TurkishVat;

/// <summary>
/// Türkiye KDV plugin sabitleri
/// </summary>
public static class TurkishVatDefaults
{
    /// <summary>Plugin system name (plugin.json ile aynı olmalı)</summary>
    public const string SystemName = "Tax.TurkishVat";

    /// <summary>Lokalizasyon kaynak prefix'i</summary>
    public const string LocaleStringResourcesPrefix = "Plugins.Tax.TurkishVat";

    /// <summary>
    /// Setting key formatı: kategori başına KDV oranı.
    /// Örn. <c>TurkishVatTax.Rate.5</c> = id'si 5 olan tax category'nin oranı.
    /// </summary>
    public const string RateSettingKey = "TurkishVatTax.Rate.{0}";

    /// <summary>
    /// 2026 yürürlükteki KDV oranları — VUK 213 sayılı kanun uyarınca
    /// Cumhurbaşkanlığı Kararnameleri ile belirlenir.
    /// Nisan 2024'te %18→%20 ve %8→%10'a yükseltildi (Karar No. 8430).
    /// </summary>
    public static class Kdv
    {
        /// <summary>%1 — temel gıda, ekmek, kitap, dergi, gazete, ekmek yapımında kullanılan un, vb.</summary>
        public const decimal IndirimliBirinci = 1m;

        /// <summary>%10 — gıda (genel), tıbbi cihaz/ilaç, tekstil, restoran-otel, sinema-tiyatro biletleri, vb.</summary>
        public const decimal IndirimliIkinci = 10m;

        /// <summary>%20 — standart oran (diğer tüm mal ve hizmetler)</summary>
        public const decimal Standart = 20m;
    }

    /// <summary>
    /// Install sırasında oluşturulacak default tax category'ler.
    /// Sıralama korunur; ilk kategori storefront'ta default seçili gelir.
    /// </summary>
    public static readonly (string Name, decimal Rate, int DisplayOrder)[] DefaultCategories =
    [
        ("KDV %20 (Standart)",         Kdv.Standart,        1),
        ("KDV %10 (İndirimli)",        Kdv.IndirimliIkinci, 2),
        ("KDV %1 (Temel Gıda/Kitap)",  Kdv.IndirimliBirinci, 3),
    ];
}
