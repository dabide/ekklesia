using System;
using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.FluentValidation;
using ServiceStack.Logging;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using ServiceStack.Validation;
using ServiceStack.Web;
using Ekklesia.Tools;
using ServiceStack.Logging.Serilog;

namespace Ekklesia
{
    public class EkklesiaAppHost : AppHostBase
    {
        private readonly AutofacIocAdapter _autofacIocAdapter;
        private ILog _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ServerEventsFeature _serverEventsFeature;
        public static IEkklesiaConfiguration Configuration { get; private set; }

        public EkklesiaAppHost(AutofacIocAdapter autofacIocAdapter, IHostingEnvironment hostingEnvironment, IConfiguration configuration, ServerEventsFeature serverEventsFeature) : base("SoundWords", typeof(EkklesiaAppHost).Assembly)
        {
            _autofacIocAdapter = autofacIocAdapter;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _serverEventsFeature = serverEventsFeature;
        }

        public override void Bind(IApplicationBuilder app)
        {
            base.Bind(app);
            // LogManager.LogFactory = new SerilogFactory();
            _logger = LogManager.GetLogger(GetType());
        }

        public override void Configure(Container container)
        {
            _logger.Debug("Configuring");

            container.Adapter = _autofacIocAdapter;
            AppSettings = container.Resolve<IAppSettings>();

            JsConfig.EmitCamelCaseNames = true;

            GlobalHtmlErrorHttpHandler = new RazorHandler("/oops");

            Configuration = container.Resolve<IEkklesiaConfiguration>();

            SetConfig(new HostConfig
            {
                DebugMode = _hostingEnvironment.IsDevelopment() || Configuration.DebugMode,
                WebHostUrl = _configuration["SiteUrl"]
            });

            Plugins.Add(new RazorFormat());
            Plugins.Add(new ValidationFeature());
            Plugins.Add(new RequestLogsFeature());

            IDbMigrator migrator = container.Resolve<IDbMigrator>();
            migrator.Migrate();
        }
    }
}
