using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class ConsentRecordBuilder : NopEntityBuilder<ConsentRecord>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // Scope, Source enum'ları: explicit AsInt32
        table
            .WithColumn(nameof(ConsentRecord.Scope)).AsInt32().NotNullable()
            .WithColumn(nameof(ConsentRecord.Source)).AsInt32().NotNullable()
            .WithColumn(nameof(ConsentRecord.TextVersion)).AsString(20).NotNullable()
            .WithColumn(nameof(ConsentRecord.IpAddress)).AsString(50).Nullable()
            .WithColumn(nameof(ConsentRecord.UserAgent)).AsString(500).Nullable()
            .WithColumn(nameof(ConsentRecord.ContentHash)).AsString(64).NotNullable();
    }
}
