using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class GeneratedContractBuilder : NopEntityBuilder<GeneratedContract>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // Type enum: explicit AsInt32
        table
            .WithColumn(nameof(GeneratedContract.Type)).AsInt32().NotNullable()
            .WithColumn(nameof(GeneratedContract.TemplateVersion)).AsString(20).NotNullable()
            .WithColumn(nameof(GeneratedContract.HtmlContent)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(GeneratedContract.AcceptanceIp)).AsString(50).Nullable()
            .WithColumn(nameof(GeneratedContract.AcceptanceUserAgent)).AsString(500).Nullable()
            .WithColumn(nameof(GeneratedContract.ContentHash)).AsString(64).NotNullable();
    }
}
