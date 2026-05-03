using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkeyCore.Data.Seed;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Migrations;

/// <summary>
/// Mahalle/posta kodu seed — embedded <c>neighborhoods.csv</c> dosyasından okur.
///
/// CSV format: <c>ProvincePlate | DistrictName | NeighborhoodName | PostalCode</c>.
///
/// Sample dataset: İstanbul Beyoğlu/Kadıköy/Beşiktaş + Ankara Çankaya + İzmir Konak.
/// Tam ~50.000+ mahalle için <c>Data/Seed/README.md</c> kaynaklara bak (PTT veri tabanı).
/// </summary>
[NopMigration("2026/05/03 12:00:00:0000004", "Misc.TurkeyCore seed neighborhoods", MigrationProcessType.Installation)]
public class SeedNeighborhoodsMigration : Migration
{
    public override void Up()
    {
        var rows = SeedCsvReader.Read("neighborhoods.csv");

        Execute.WithConnection((conn, tx) =>
        {
            var districtLookup = SeedDataHelper.LoadDistrictLookup(conn, tx);

            foreach (var row in rows)
            {
                if (row.Length < 4 || !int.TryParse(row[0], out var plate))
                    continue;

                var districtName = row[1];
                var name = row[2];
                var postalCode = row[3];

                if (string.IsNullOrWhiteSpace(districtName) ||
                    string.IsNullOrWhiteSpace(name))
                    continue;

                if (!districtLookup.TryGetValue((plate, districtName), out var districtId))
                    continue;

                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = $"INSERT INTO {nameof(TurkishNeighborhood)} " +
                                   "(DistrictId, Name, PostalCode, Active) " +
                                   "VALUES (@d, @n, @p, @a)";
                SeedDataHelper.AddParam(cmd, "@d", districtId);
                SeedDataHelper.AddParam(cmd, "@n", name);
                SeedDataHelper.AddParam(cmd, "@p", postalCode ?? string.Empty);
                SeedDataHelper.AddParam(cmd, "@a", true);
                cmd.ExecuteNonQuery();
            }
        });
    }

    public override void Down() { }
}
