using System.Data;
using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkeyCore.Data.Seed;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Migrations;

/// <summary>
/// İlçe seed — embedded <c>districts.csv</c> dosyasından okur.
///
/// CSV format: <c>ProvincePlateCode | DistrictName</c>. Province lookup runtime'da yapılır
/// (önce <see cref="SeedProvincesMigration"/> çalıştığı için iller mevcuttur).
///
/// Sample dataset: İstanbul/Ankara/İzmir tam + büyük şehirlerden örnek.
/// Tam ~973 ilçe için <c>Data/Seed/README.md</c> kaynaklara bak.
/// </summary>
[NopMigration("2026/05/03 12:00:00:0000003", "Misc.TurkeyCore seed districts", MigrationProcessType.Installation)]
public class SeedDistrictsMigration : Migration
{
    public override void Up()
    {
        var rows = SeedCsvReader.Read("districts.csv");

        Execute.WithConnection((conn, tx) =>
        {
            var plateToId = SeedDataHelper.LoadProvincePlateToIdMap(conn, tx);
            var perProvinceOrder = new Dictionary<int, int>();

            foreach (var row in rows)
            {
                if (row.Length < 2 || !int.TryParse(row[0], out var plate))
                    continue;

                if (!plateToId.TryGetValue(plate, out var provinceId))
                    continue;

                var name = row[1];
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                perProvinceOrder.TryGetValue(provinceId, out var order);
                perProvinceOrder[provinceId] = ++order;

                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = $"INSERT INTO {nameof(TurkishDistrict)} " +
                                   "(ProvinceId, Name, DisplayOrder, Active) " +
                                   "VALUES (@p, @n, @d, @a)";
                SeedDataHelper.AddParam(cmd, "@p", provinceId);
                SeedDataHelper.AddParam(cmd, "@n", name);
                SeedDataHelper.AddParam(cmd, "@d", order);
                SeedDataHelper.AddParam(cmd, "@a", true);
                cmd.ExecuteNonQuery();
            }
        });
    }

    public override void Down()
    {
        // Schema migration tabloyu drop ettiği için ek temizliğe gerek yok
    }
}
