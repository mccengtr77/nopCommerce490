using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

[NopMigration("2026/05/03 13:00:00:0000008",
    "Misc.TurkishConsumerLaw contracts (MSS + ÖBF) schema",
    MigrationProcessType.Installation)]
public class ContractsSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<ContractTemplate>();
        Create.TableFor<GeneratedContract>();
    }
}
