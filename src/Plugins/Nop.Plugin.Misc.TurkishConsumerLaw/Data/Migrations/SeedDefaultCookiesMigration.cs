using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

/// <summary>
/// nopCommerce'in standart çerez tanımlarını ve sık karşılaşılan üçüncü taraf
/// çerezlerini seed eder. Admin panelinden düzenlenebilir/silinebilir.
/// </summary>
[NopMigration("2026/05/03 13:00:00:0000003",
    "Misc.TurkishConsumerLaw seed default cookie definitions",
    MigrationProcessType.Installation)]
public class SeedDefaultCookiesMigration : Migration
{
    public override void Up()
    {
        var tableName = nameof(CookieDefinition);

        var cookies = new (string Name, string Provider, string Purpose, string Duration, CookieCategory Category, bool IsThirdParty, int Order)[]
        {
            // Zorunlu — nopCommerce default
            (".Nop.Authentication", "Site içi", "Müşteri oturum yönetimi", "Oturum sonu", CookieCategory.Necessary, false, 10),
            (".Nop.Antiforgery", "Site içi", "CSRF token (form güvenliği)", "Oturum sonu", CookieCategory.Necessary, false, 20),
            ("Nop.customer", "Site içi", "Misafir müşteri tanımlama (sepet vb.)", "365 gün", CookieCategory.Necessary, false, 30),
            ("Nop.RecentlyViewedProducts", "Site içi", "Son görüntülenen ürünler", "10 gün", CookieCategory.Functional, false, 40),
            ("Nop.compareproducts", "Site içi", "Ürün karşılaştırma listesi", "Oturum sonu", CookieCategory.Functional, false, 50),

            // İşlevsellik
            ("Nop.preferenceLanguageId", "Site içi", "Seçili dil tercihi", "365 gün", CookieCategory.Functional, false, 60),
            ("Nop.preferenceCurrencyId", "Site içi", "Seçili para birimi tercihi", "365 gün", CookieCategory.Functional, false, 70),

            // Plugin içi
            ("tcl-consent", "Site içi (Türkiye Tüketici Hukuku)", "Çerez onay tercihinizi saklar", "180 gün", CookieCategory.Necessary, false, 5),

            // Analitik (yaygın 3rd party — admin'in kendi setup'ına göre kullanılır)
            ("_ga", "Google Analytics", "Ziyaretçi ayrımı (anonim)", "2 yıl", CookieCategory.Analytics, true, 80),
            ("_gid", "Google Analytics", "Ziyaretçi oturumu", "24 saat", CookieCategory.Analytics, true, 90),

            // Pazarlama
            ("_fbp", "Facebook (Meta)", "Pixel — reklam ölçüm ve hedefleme", "90 gün", CookieCategory.Marketing, true, 100),
            ("_gcl_au", "Google Ads", "Conversion tracking", "90 gün", CookieCategory.Marketing, true, 110)
        };

        foreach (var (name, provider, purpose, duration, category, thirdParty, order) in cookies)
        {
            Insert.IntoTable(tableName).Row(new
            {
                Name = name,
                Provider = provider,
                Purpose = purpose,
                Duration = duration,
                Category = (int)category,
                IsThirdParty = thirdParty,
                IsActive = true,
                DisplayOrder = order
            });
        }
    }

    public override void Down()
    {
        // Schema migration tabloyu drop ettiği için ek temizliğe gerek yok
    }
}
