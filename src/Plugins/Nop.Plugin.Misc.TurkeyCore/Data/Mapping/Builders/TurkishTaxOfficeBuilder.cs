using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

public class TurkishTaxOfficeBuilder : NopEntityBuilder<TurkishTaxOffice>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TurkishTaxOffice.ProvinceId)).AsInt32().NotNullable().ForeignKey<TurkishProvince>()
            .WithColumn(nameof(TurkishTaxOffice.Name)).AsString(250).NotNullable()
            .WithColumn(nameof(TurkishTaxOffice.Code)).AsString(10).NotNullable();
    }
}
