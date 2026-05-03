using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Etbis;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Data.Migrations;

/// <summary>
/// TurkishConsumerLaw plugin başlangıç şema migration'ı.
/// Şu an sadece ETBİS tablosunu oluşturur. Diğer modüller (Contracts, Withdrawal,
/// KVKK, Cookies, İYS, Warranty, Complaints) ayrı migration'larda eklenir —
/// timestamp sırasına göre uygulanır.
/// </summary>
[NopMigration("2026/05/03 13:00:00:0000001",
    "Misc.TurkishConsumerLaw base schema (Etbis)",
    MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<EtbisRegistration>();
    }
}
