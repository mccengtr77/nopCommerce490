using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Migrations;

/// <summary>
/// TurkishAddressExtension tablosuna faturalama alanlarını ekler:
///  - <see cref="TurkishAddressExtension.MusteriTipi"/> (bireysel/kurumsal)
///  - <see cref="TurkishAddressExtension.TcKimlikNo"/> (şifreli, bireysel)
///  - <see cref="TurkishAddressExtension.VergiNo"/> (şifreli, kurumsal)
///  - <see cref="TurkishAddressExtension.VergiDairesiId"/> (FK soft, kurumsal)
///
/// Idempotent: kolonlar zaten varsa atlar — fresh install'da SchemaMigration zaten
/// bu kolonları builder üzerinden oluşturduğu için bu migration kayıt edilmiş olarak işaretlenir
/// ve gerçek ALTER yapılmaz.
/// </summary>
[NopMigration("2026/05/04 09:00:00", "Misc.TurkeyCore: TurkishAddressExtension faturalama alanları",
    MigrationProcessType.Update)]
public class AddInvoiceFieldsToAddressExtensionMigration : ForwardOnlyMigration
{
    private const string TableName = nameof(TurkishAddressExtension);

    public override void Up()
    {
        if (!Schema.Table(TableName).Column(nameof(TurkishAddressExtension.AdresAdi)).Exists())
        {
            Alter.Table(TableName)
                .AddColumn(nameof(TurkishAddressExtension.AdresAdi)).AsString(100).Nullable();
        }

        if (!Schema.Table(TableName).Column(nameof(TurkishAddressExtension.MusteriTipi)).Exists())
        {
            Alter.Table(TableName)
                .AddColumn(nameof(TurkishAddressExtension.MusteriTipi)).AsInt32().NotNullable().WithDefaultValue(1);
        }

        if (!Schema.Table(TableName).Column(nameof(TurkishAddressExtension.TcKimlikNo)).Exists())
        {
            Alter.Table(TableName)
                .AddColumn(nameof(TurkishAddressExtension.TcKimlikNo)).AsString(256).Nullable();
        }

        if (!Schema.Table(TableName).Column(nameof(TurkishAddressExtension.VergiNo)).Exists())
        {
            Alter.Table(TableName)
                .AddColumn(nameof(TurkishAddressExtension.VergiNo)).AsString(256).Nullable();
        }

        if (!Schema.Table(TableName).Column(nameof(TurkishAddressExtension.VergiDairesiId)).Exists())
        {
            Alter.Table(TableName)
                .AddColumn(nameof(TurkishAddressExtension.VergiDairesiId)).AsInt32().Nullable();
            Create.Index().OnTable(TableName)
                .OnColumn(nameof(TurkishAddressExtension.VergiDairesiId)).Ascending();
        }
    }
}
