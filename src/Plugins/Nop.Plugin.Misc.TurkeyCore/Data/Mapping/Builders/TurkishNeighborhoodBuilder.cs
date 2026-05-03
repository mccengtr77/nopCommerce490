using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

public class TurkishNeighborhoodBuilder : NopEntityBuilder<TurkishNeighborhood>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TurkishNeighborhood.DistrictId)).AsInt32().NotNullable().ForeignKey<TurkishDistrict>()
            .WithColumn(nameof(TurkishNeighborhood.Name)).AsString(200).NotNullable()
            .WithColumn(nameof(TurkishNeighborhood.PostalCode)).AsString(5).NotNullable();
    }
}
