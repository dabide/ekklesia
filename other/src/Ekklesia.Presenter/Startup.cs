using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Ekklesia.Audio;
using Ekklesia.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;

namespace Ekklesia
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSignalR();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new EkklesiaModule(Configuration));
            builder.RegisterModule<SerilogAutofacTraceModule>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILifetimeScope lifetimeScope, IApplicationLifetime applicationLifetime)
        {
            LicenseUtils.RegisterLicense(Configuration["servicestack:license"]);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ConfigFile = "webpack.netcore.config.js",
                    HotModuleReplacementClientOptions = new Dictionary<string, string>{
                    {"reload", "true"}
                  }
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<SongHub>("song");
            });

            app.UseServiceStack(lifetimeScope.Resolve<AppHostBase>());

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            lifetimeScope.Resolve<IWebcaster>().Start(8003, cancellationTokenSource.Token);

            applicationLifetime.ApplicationStopping.Register(() =>
            {
                cancellationTokenSource.Cancel();
            });
        }
    }
}
