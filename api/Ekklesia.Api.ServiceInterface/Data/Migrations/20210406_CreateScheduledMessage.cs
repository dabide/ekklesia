using FluentMigrator;
using JetBrains.Annotations;

namespace Ekklesia.Api.Data.Migrations
{
    [Migration(20210406015800)]
    [UsedImplicitly]
    public class CreateScheduledMessage : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("ScheduledMessage")
                .WithDefaultColumns()
                .WithCompositeData()
                .WithColumn("Description").AsString();
        }
    }
}