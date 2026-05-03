using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class ContractTemplateBuilder : NopEntityBuilder<ContractTemplate>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // Type enum: explicit AsInt32
        table
            .WithColumn(nameof(ContractTemplate.Type)).AsInt32().NotNullable()
            .WithColumn(nameof(ContractTemplate.Name)).AsString(255).NotNullable()
            .WithColumn(nameof(ContractTemplate.HtmlContent)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(ContractTemplate.Version)).AsString(20).NotNullable();
    }
}
