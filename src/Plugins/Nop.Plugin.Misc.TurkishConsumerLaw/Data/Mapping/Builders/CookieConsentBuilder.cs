using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Mapping.Builders;

public class CookieConsentBuilder : NopEntityBuilder<CookieConsent>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(CookieConsent.PolicyVersion)).AsString(20).NotNullable()
            .WithColumn(nameof(CookieConsent.IpAddress)).AsString(50).Nullable()
            .WithColumn(nameof(CookieConsent.UserAgent)).AsString(500).Nullable()
            .WithColumn(nameof(CookieConsent.ContentHash)).AsString(64).NotNullable();
    }
}
