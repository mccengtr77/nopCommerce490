using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Migrations;

/// <summary>
/// <see cref="TurkishCartItemPriceLock"/> tablosunu ekler — sepete eklenen ürün için
/// o anki TRY fiyat snapshot'ı.
///
/// Idempotent: Fresh install'da <see cref="SchemaMigration"/> bu tabloyu oluşturduktan sonra
/// bu migration kayıt edilmiş olarak işaretlenir.
/// </summary>
[NopMigration("2026/05/04 12:30:00", "Misc.TurkeyCore: TurkishCartItemPriceLock tablosu",
    MigrationProcessType.Update)]
public class AddCartItemPriceLockMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(TurkishCartItemPriceLock)).Exists())
        {
            Create.TableFor<TurkishCartItemPriceLock>();
        }
    }
}
