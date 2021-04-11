using System;
using Autofac;
using Ekklesia.Api.ServiceInterface;
using FluentMigrator.Runner;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ServiceStack;

namespace Ekklesia.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => ConfigureMigrationRunner(rb)
                    .WithGlobalConnectionString(Configuration.GetConnectionString("DefaultConnection"))
                    .ScanIn(typeof(ServiceInterfaceModule).Assembly).For.Migrations()
                );
        }

        [UsedImplicitly]
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new ApiModule(Configuration));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            LicenseUtils.RegisterLicense(Configuration["servicestack:license"]);

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseServiceStack(new AppHost
            {
                AppSettings = new NetCoreAppSettings(Configuration)
            });
        }
        
        private IMigrationRunnerBuilder ConfigureMigrationRunner(IMigrationRunnerBuilder migrationRunnerBuilder)
        {
            switch (Configuration.GetValue("DbType", "Sqlite"))
            {
                case "SQLServer":
                    migrationRunnerBuilder.AddSqlServer2016();
                    break;
                case "MySQL":
                    migrationRunnerBuilder.AddMySql5();
                    break;
                case "PostgreSQL":
                    migrationRunnerBuilder.AddPostgres();
                    break;
                case "Sqlite":
                    migrationRunnerBuilder.AddSQLite();
                    break;
            }

            return migrationRunnerBuilder;
        }
    }
}