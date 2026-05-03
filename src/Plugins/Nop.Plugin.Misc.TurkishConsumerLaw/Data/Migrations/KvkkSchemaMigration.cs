using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

[NopMigration("2026/05/03 13:00:00:0000004",
    "Misc.TurkishConsumerLaw KVKK schema",
    MigrationProcessType.Installation)]
public class KvkkSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<DisclosureText>();
        Create.TableFor<ExplicitConsentText>();
        Create.TableFor<ConsentRecord>();
    }
}
