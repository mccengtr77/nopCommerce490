using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class CookieDefinitionBuilder : NopEntityBuilder<CookieDefinition>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // Category enum: explicit AsInt32 (auto-generation plugin enum'larını atlıyor)
        table
            .WithColumn(nameof(CookieDefinition.Name)).AsString(255).NotNullable()
            .WithColumn(nameof(CookieDefinition.Provider)).AsString(255).Nullable()
            .WithColumn(nameof(CookieDefinition.Purpose)).AsString(1000).Nullable()
            .WithColumn(nameof(CookieDefinition.Duration)).AsString(100).Nullable()
            .WithColumn(nameof(CookieDefinition.Category)).AsInt32().NotNullable();
    }
}
