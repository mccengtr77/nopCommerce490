using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.DataSubjectRequests;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

[NopMigration("2026/05/03 13:00:00:0000006",
    "Misc.TurkishConsumerLaw KVKK m.11 data subject request schema",
    MigrationProcessType.Installation)]
public class DataSubjectRequestSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<DataSubjectRequest>();
    }
}
