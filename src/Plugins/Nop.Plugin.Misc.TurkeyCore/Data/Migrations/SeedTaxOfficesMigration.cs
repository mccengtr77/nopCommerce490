using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkeyCore.Data.Seed;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Migrations;

/// <summary>
/// Vergi daireleri seed — embedded <c>taxoffices.csv</c> dosyasından okur.
///
/// CSV format: <c>ProvincePlate | TaxOfficeName | GibCode</c>.
///
/// Sample dataset: İstanbul (32) + Ankara (15) + İzmir (10) + Bursa/Antalya örnek.
/// Tam ~1000 vergi dairesi için <c>Data/Seed/README.md</c> kaynaklara bak (GİB resmi liste).
/// </summary>
[NopMigration("2026/05/03 12:00:00:0000005", "Misc.TurkeyCore seed tax offices", MigrationProcessType.Installation)]
public class SeedTaxOfficesMigration : Migration
{
    public override void Up()
    {
        var rows = SeedCsvReader.Read("taxoffices.csv");

        Execute.WithConnection((conn, tx) =>
        {
            var plateToId = SeedDataHelper.LoadProvincePlateToIdMap(conn, tx);

            foreach (var row in rows)
            {
                if (row.Length < 3 || !int.TryParse(row[0], out var plate))
                    continue;

                var name = row[1];
                var code = row[2];

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!plateToId.TryGetValue(plate, out var provinceId))
                    continue;

                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = $"INSERT INTO {nameof(TurkishTaxOffice)} " +
                                   "(ProvinceId, Name, Code, Active) " +
                                   "VALUES (@p, @n, @c, @a)";
                SeedDataHelper.AddParam(cmd, "@p", provinceId);
                SeedDataHelper.AddParam(cmd, "@n", name);
                SeedDataHelper.AddParam(cmd, "@c", code ?? string.Empty);
                SeedDataHelper.AddParam(cmd, "@a", true);
                cmd.ExecuteNonQuery();
            }
        });
    }

    public override void Down() { }
}
