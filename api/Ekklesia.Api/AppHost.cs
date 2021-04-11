using Ekklesia.Api.Data.Models;
using Ekklesia.Api.ServiceInterface;
using FluentMigrator.Runner;
using Funq;
using NodaTime;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Validation;

namespace Ekklesia.Api
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("Ekklesia.Api", typeof(MyServices).Assembly)
        {
        }

        // Configure your AppHost with the necessary configuration and dependencies your App needs
        public override void Configure(Container container)
        {
            SetConfig(new HostConfig
            {
                DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), false)
            });

            Plugins.Add(new ValidationFeature());
            Plugins.Add(new RequestLogsFeature());
            Plugins.Add(new ServerEventsFeature());

            IMigrationRunner migrator = container.Resolve<IMigrationRunner>();
            migrator.MigrateUp();

            OrmLiteConfig.InsertFilter = (_, row) =>
            {
                if (row is IHaveAuditData auditRow) auditRow.CreatedOn = auditRow.ModifiedOn = container.Resolve<IClock>().GetCurrentInstant();
            };

            OrmLiteConfig.UpdateFilter = (_, row) =>
            {
                if (row is IHaveAuditData auditRow) auditRow.ModifiedOn = container.Resolve<IClock>().GetCurrentInstant();
            };
        }
    }
}
