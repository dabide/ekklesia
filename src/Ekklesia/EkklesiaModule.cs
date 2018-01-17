using System;
using System.IO.Abstractions;
using Autofac;
using AutofacSerilogIntegration;
using Ekklesia.Tools;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using ServiceStack.Caching;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;

namespace Ekklesia
{
    internal class EkklesiaModule : Module
    {
        private readonly IConfiguration _configuration;

        public EkklesiaModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(EkklesiaModule).Assembly)
                      .Except<EkklesiaConfiguration>(register => register
                                                           .As<IEkklesiaConfiguration>()
                                                           .As<IAppSettings>()
                                                           .SingleInstance())
                      .Except<EkklesiaAppHost>(register => register.As<AppHostBase>().SingleInstance())
                      .Except<BackgroundPool>(register => register.As<IBackgroundPool>().SingleInstance())
                      .AsDefaultInterface()
                      .AsSelf();

            builder.RegisterType<FileSystem>().As<IFileSystem>();

            builder.RegisterType<MemoryCacheClient>().As<ICacheClient>().SingleInstance();


            // builder.Register(context =>
            //                  {
            //                      IOrmLiteDialectProvider dialectProvider = GetDialectProvider(_configuration["DbType"]);
            //                      OrmLiteConnectionFactory connectionFactory = new OrmLiteConnectionFactory(
            //                          _configuration.GetConnectionString("DefaultConnection"),
            //                          dialectProvider);
            //                      connectionFactory.RegisterConnection(
            //                          "Users", _configuration.GetConnectionString("UsersConnection") ?? _configuration.GetConnectionString("DefaultConnection"),
            //                          dialectProvider);
            //                      return connectionFactory;
            //                  })
            //        .As<IDbConnectionFactory>()
            //        .SingleInstance();

            builder.RegisterLogger();

            builder.Register(c => _configuration).As<IConfiguration>();

            builder.Register(c => new ServerEventsFeature())
                   .AsSelf()
                   .SingleInstance();

            builder.Register(c =>
                             {
                                 ServerEventsFeature serverEventsFeature = c.Resolve<ServerEventsFeature>();
                                 return new MemoryServerEvents
                                 {
                                     IdleTimeout = serverEventsFeature.IdleTimeout,
                                     HouseKeepingInterval = serverEventsFeature.HouseKeepingInterval,
                                     OnSubscribe = serverEventsFeature.OnSubscribe,
                                     OnUnsubscribe = serverEventsFeature.OnUnsubscribe,
                                     NotifyChannelOfSubscriptions = serverEventsFeature.NotifyChannelOfSubscriptions,
                                     OnError = serverEventsFeature.OnError
                                 };
                             }).As<IServerEvents>()
                   .SingleInstance();
        }

        private static IOrmLiteDialectProvider GetDialectProvider(string dbType)
        {
            switch (dbType)
            {
                case "SQLServer":
                    return SqlServerDialect.Provider;
                case "MySQL":
                    return MySqlDialect.Provider;
                case "PostgreSQL":
                    return PostgreSqlDialect.Provider;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dbType),
                                                          "The database type is not supported");
            }
        }
    }
}
