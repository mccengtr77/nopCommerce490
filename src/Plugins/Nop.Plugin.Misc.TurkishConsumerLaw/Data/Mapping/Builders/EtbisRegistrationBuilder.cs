using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Etbis;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class EtbisRegistrationBuilder : NopEntityBuilder<EtbisRegistration>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(EtbisRegistration.MersisNo)).AsString(20).NotNullable()
            .WithColumn(nameof(EtbisRegistration.TradeName)).AsString(500).NotNullable()
            .WithColumn(nameof(EtbisRegistration.VerificationUrl)).AsString(500).Nullable()
            // QR kodu inline SVG olabilir → büyük; nvarchar(max)
            .WithColumn(nameof(EtbisRegistration.QrCodeHtml)).AsString(int.MaxValue).Nullable();
    }
}
