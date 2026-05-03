using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class DataSubjectRequestBuilder : NopEntityBuilder<DataSubjectRequest>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // RequestType, Status enum'ları: explicit AsInt32
        table
            .WithColumn(nameof(DataSubjectRequest.RequestType)).AsInt32().NotNullable()
            .WithColumn(nameof(DataSubjectRequest.Status)).AsInt32().NotNullable()
            .WithColumn(nameof(DataSubjectRequest.FullName)).AsString(255).NotNullable()
            .WithColumn(nameof(DataSubjectRequest.Email)).AsString(255).NotNullable()
            .WithColumn(nameof(DataSubjectRequest.Phone)).AsString(50).Nullable()
            .WithColumn(nameof(DataSubjectRequest.Description)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(DataSubjectRequest.AdminResponse)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(DataSubjectRequest.IpAddress)).AsString(50).Nullable()
            .WithColumn(nameof(DataSubjectRequest.UserAgent)).AsString(500).Nullable();
    }
}
