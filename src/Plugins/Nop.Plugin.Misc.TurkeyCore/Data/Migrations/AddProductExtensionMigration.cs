using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Migrations;

/// <summary>
/// <see cref="TurkishProductExtension"/> tablosunu ekler — ürün başına döviz bazlı fiyat tanımı.
///
/// Idempotent: Fresh install'da <see cref="SchemaMigration"/> bu tabloyu oluşturduktan sonra
/// migration kayıt edilmiş olarak işaretlenir; tablo zaten varsa <c>Create.TableFor</c> atlanır.
/// </summary>
[NopMigration("2026/05/04 12:00:00", "Misc.TurkeyCore: TurkishProductExtension tablosu",
    MigrationProcessType.Update)]
public class AddProductExtensionMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(TurkishProductExtension)).Exists())
        {
            Create.TableFor<TurkishProductExtension>();
        }
    }
}
