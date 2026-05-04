using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Migrations;

/// <summary>
/// TurkeyCore plugin başlangıç şema migration — tüm Türkiye-spesifik tabloları oluşturur.
/// Seed kayıtları (81 il, ~970 ilçe, ~50.000 mahalle, ~1000 vergi dairesi) ayrı migration'larda yapılır.
/// </summary>
[NopMigration("2026/05/03 12:00:00:0000001", "Misc.TurkeyCore base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        // Lokasyon tabloları (üst-alt sırasıyla, FK constraint'ler için)
        Create.TableFor<TurkishProvince>();
        Create.TableFor<TurkishDistrict>();
        Create.TableFor<TurkishNeighborhood>();

        // Vergi daireleri (il'e bağlı)
        Create.TableFor<TurkishTaxOffice>();

        // Customer & Address extension tabloları
        Create.TableFor<TurkishCustomerExtension>();
        Create.TableFor<TurkishAddressExtension>();

        // Product extension — ürün başına döviz bazlı fiyat
        Create.TableFor<TurkishProductExtension>();

        // Cart item price lock — sepete eklenince TL fiyat snapshot
        Create.TableFor<TurkishCartItemPriceLock>();

        // TCMB döviz kuru log
        Create.TableFor<ExchangeRateLog>();
    }
}
