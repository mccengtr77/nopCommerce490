using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class WithdrawalRequestBuilder : NopEntityBuilder<WithdrawalRequest>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // Reason, Status enum'ları: explicit AsInt32
        table
            .WithColumn(nameof(WithdrawalRequest.Reason)).AsInt32().NotNullable()
            .WithColumn(nameof(WithdrawalRequest.Status)).AsInt32().NotNullable()
            .WithColumn(nameof(WithdrawalRequest.Description)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(WithdrawalRequest.RejectionReason)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(WithdrawalRequest.ReturnTrackingNumber)).AsString(100).Nullable()
            .WithColumn(nameof(WithdrawalRequest.InspectionNotes)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(WithdrawalRequest.RefundAmount)).AsDecimal(18, 4).Nullable()
            .WithColumn(nameof(WithdrawalRequest.IpAddress)).AsString(50).Nullable()
            .WithColumn(nameof(WithdrawalRequest.UserAgent)).AsString(500).Nullable();
    }
}
