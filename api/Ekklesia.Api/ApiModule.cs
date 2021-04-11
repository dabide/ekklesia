using System;
using System.IO.Abstractions;
using Autofac;
using Ekklesia.Api.Data;
using Ekklesia.Api.Common.Extensions;
using Microsoft.Extensions.Configuration;
using NodaTime;
using ServiceStack;
using ServiceStack.Caching;
using ServiceStack.Data;
using ServiceStack.Messaging;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.MySql;
using ServiceStack.OrmLite.PostgreSQL;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.RabbitMq;

namespace Ekklesia.Api
{
    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;

        public ApiModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .WithPublicConstructors()
                .AsDefaultInterface()
                .AsSelf();

            builder.RegisterType<FileSystem>().As<IFileSystem>();

            builder.Register(_ => SystemClock.Instance).As<IClock>();
            
            builder.RegisterType<MemoryCacheClient>().As<ICacheClient>().SingleInstance();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            builder.Register(_ =>
                {
                    string dbType = _configuration.GetValue("DbType", "Sqlite");
                    OrmLiteConnectionFactory connectionFactory = new(
                        connectionString,
                        dbType switch
                        {
                            "SQLServer" => SqlServer2019OrmLiteDialectProvider.Instance,
                            "MySQL" => MySqlConnectorDialectProvider.Instance,
                            "PostgreSQL" => PostgreSqlDialectProvider.Instance,
                            "Sqlite" => SqliteOrmLiteDialectProvider.Instance,
                            _ => throw new ArgumentOutOfRangeException(nameof(dbType), "The database type is not supported")
                        });
                    
                    IOrmLiteConverter dateTimeConverter = connectionFactory.DialectProvider.GetConverter<DateTime>();
                    connectionFactory.DialectProvider.RegisterConverter<Instant>(new InstantConverter(dateTimeConverter));

                    if (connectionFactory.DialectProvider == SqliteOrmLiteDialectProvider.Instance)
                    {
                        connectionFactory.ConnectionString =
                            connectionFactory.ConnectionString.Replace("Data Source=", string.Empty);
                    }
                    return connectionFactory;
                })
                .As<IDbConnectionFactory>()
                .SingleInstance();

            
            builder.Register(_ => new ServerEventsFeature())
                .AsSelf()
                .SingleInstance();

            builder.Register(_ => new RabbitMqServer(_configuration.GetValue("RABBITMQ_SERVER", "localhost")))
                .As<IMessageService>()
                .AsSelf()
                .SingleInstance();

            builder.Register(c =>
                {
                    ServerEventsFeature serverEventsFeature = c.Resolve<ServerEventsFeature>();
                    return new MemoryServerEvents
                    {
                        IdleTimeout = serverEventsFeature.IdleTimeout,
                        HouseKeepingInterval = serverEventsFeature.HouseKeepingInterval,
                        OnSubscribeAsync = serverEventsFeature.OnSubscribeAsync,
                        OnUnsubscribeAsync = serverEventsFeature.OnUnsubscribeAsync,
                        NotifyChannelOfSubscriptions = serverEventsFeature.NotifyChannelOfSubscriptions,
                        OnError = serverEventsFeature.OnError
                    };
                }).As<IServerEvents>()
                .SingleInstance();
        }
    }
}