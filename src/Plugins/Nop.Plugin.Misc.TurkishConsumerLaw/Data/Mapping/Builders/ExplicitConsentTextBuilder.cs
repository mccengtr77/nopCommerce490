using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class ExplicitConsentTextBuilder : NopEntityBuilder<ExplicitConsentText>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // Scope enum: explicit AsInt32
        table
            .WithColumn(nameof(ExplicitConsentText.Scope)).AsInt32().NotNullable()
            .WithColumn(nameof(ExplicitConsentText.ShortLabel)).AsString(500).NotNullable()
            .WithColumn(nameof(ExplicitConsentText.Content)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(ExplicitConsentText.Version)).AsString(20).NotNullable();
    }
}
