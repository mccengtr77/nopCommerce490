using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class DisclosureTextBuilder : NopEntityBuilder<DisclosureText>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DisclosureText.Title)).AsString(255).NotNullable()
            .WithColumn(nameof(DisclosureText.Content)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(DisclosureText.Version)).AsString(20).NotNullable();
    }
}
