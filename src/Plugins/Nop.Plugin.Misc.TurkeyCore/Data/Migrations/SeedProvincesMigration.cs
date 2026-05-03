using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkeyCore.Data.Seed;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Migrations;

/// <summary>
/// 81 il seed migration — embedded <c>provinces.csv</c> dosyasından okur.
/// Liste İçişleri Bakanlığı resmi plaka kodlarına göredir.
/// İlçe, mahalle ve vergi dairesi seed'leri ardışık migration'larda yapılır.
/// </summary>
[NopMigration("2026/05/03 12:00:00:0000002", "Misc.TurkeyCore seed provinces", MigrationProcessType.Installation)]
public class SeedProvincesMigration : Migration
{
    public override void Up()
    {
        var rows = SeedCsvReader.Read("provinces.csv");

        // Türkçe culture-sensitive alfabetik sıralama (DisplayOrder için)
        var trCulture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
        var trComparer = StringComparer.Create(trCulture, false);

        var ordered = rows
            .Where(r => r.Length >= 2 && int.TryParse(r[0], out _))
            .Select(r => (Plate: int.Parse(r[0]), Name: r[1]))
            .OrderBy(p => p.Name, trComparer)
            .Select((p, i) => (p.Plate, p.Name, DisplayOrder: i + 1))
            .ToArray();

        var tableName = nameof(TurkishProvince);
        foreach (var (plate, name, displayOrder) in ordered)
        {
            Insert.IntoTable(tableName).Row(new
            {
                Name = name,
                PlateCode = plate,
                DisplayOrder = displayOrder,
                Active = true
            });
        }
    }

    public override void Down()
    {
        // Schema migration tabloyu drop ettiği için ek temizliğe gerek yok
    }
}
