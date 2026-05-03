using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

public class ExchangeRateLogBuilder : NopEntityBuilder<ExchangeRateLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ExchangeRateLog.CurrencyCode)).AsString(3).NotNullable()
            .WithColumn(nameof(ExchangeRateLog.ForexBuying)).AsDecimal(18, 6).NotNullable()
            .WithColumn(nameof(ExchangeRateLog.ForexSelling)).AsDecimal(18, 6).NotNullable()
            .WithColumn(nameof(ExchangeRateLog.BanknoteBuying)).AsDecimal(18, 6).Nullable()
            .WithColumn(nameof(ExchangeRateLog.BanknoteSelling)).AsDecimal(18, 6).Nullable();
    }
}
