using System;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using ServiceStack.Logging;

namespace Ekklesia.Tools
{
    public class DbMigrator : IDbMigrator
    {
        private readonly IConfiguration _configuration;

        public DbMigrator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Migrate()
        {
            IAnnouncer announcer = new MigrationAnnouncer() {ShowSql = true};
            IMigrationProcessorFactory migrationProcessorFactory = new SQLiteProcessorFactory();

            ProcessorOptions options = new ProcessorOptions
                                       {
                                           PreviewOnly = false, // set to true to see the SQL
                                           Timeout = 60
                                       };

            using (IMigrationProcessor processor =
                migrationProcessorFactory.Create(_configuration.GetConnectionString("DefaultConnection"), announcer,
                                                 options))
            {
                MigrationRunner runner = new MigrationRunner(GetType().Assembly, new RunnerContext(announcer), processor);
                runner.MigrateUp();
            }
        }
    }
}
