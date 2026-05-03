using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

public class TurkishDistrictBuilder : NopEntityBuilder<TurkishDistrict>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TurkishDistrict.ProvinceId)).AsInt32().NotNullable().ForeignKey<TurkishProvince>()
            .WithColumn(nameof(TurkishDistrict.Name)).AsString(150).NotNullable();
    }
}
