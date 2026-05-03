using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

public class TurkishCustomerExtensionBuilder : NopEntityBuilder<TurkishCustomerExtension>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // TCKN/VKN şifrelenmiş halde saklandığı için max ~256 (string IEncryptionService çıktısı için ihtiyatlı)
        // VergiDairesiId: soft FK (multi-cascade path riskini önlemek için just Indexed)
        // MusteriTipi enum: explicit AsInt32 — auto-generation enum'ları skip ediyor
        table
            .WithColumn(nameof(TurkishCustomerExtension.CustomerId)).AsInt32().NotNullable().ForeignKey<Customer>()
            .WithColumn(nameof(TurkishCustomerExtension.TcKimlikNo)).AsString(256).Nullable()
            .WithColumn(nameof(TurkishCustomerExtension.VergiNo)).AsString(256).Nullable()
            .WithColumn(nameof(TurkishCustomerExtension.VergiDairesiId)).AsInt32().Nullable().Indexed()
            .WithColumn(nameof(TurkishCustomerExtension.Mersis)).AsString(20).Nullable()
            .WithColumn(nameof(TurkishCustomerExtension.TicaretSicilNo)).AsString(50).Nullable()
            .WithColumn(nameof(TurkishCustomerExtension.KepAdresi)).AsString(255).Nullable()
            .WithColumn(nameof(TurkishCustomerExtension.MusteriTipi)).AsInt32().NotNullable();
    }
}
