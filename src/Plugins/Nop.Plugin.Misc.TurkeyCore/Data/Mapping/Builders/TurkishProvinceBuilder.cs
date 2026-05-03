using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

public class TurkishProvinceBuilder : NopEntityBuilder<TurkishProvince>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TurkishProvince.Name)).AsString(100).NotNullable()
            .WithColumn(nameof(TurkishProvince.PlateCode)).AsInt32().NotNullable();
    }
}
