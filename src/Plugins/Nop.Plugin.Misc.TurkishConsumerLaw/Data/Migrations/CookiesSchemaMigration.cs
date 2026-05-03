using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

/// <summary>
/// Çerez consent modülü tabloları.
/// </summary>
[NopMigration("2026/05/03 13:00:00:0000002",
    "Misc.TurkishConsumerLaw cookie consent schema",
    MigrationProcessType.Installation)]
public class CookiesSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<CookieDefinition>();
        Create.TableFor<CookieConsent>();
    }
}
