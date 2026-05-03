using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

[NopMigration("2026/05/03 13:00:00:0000007",
    "Misc.TurkishConsumerLaw withdrawal schema",
    MigrationProcessType.Installation)]
public class WithdrawalSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<WithdrawalRequest>();
    }
}
