using System;
using Silky.Lms.Core;
using Silky.Lms.HttpServer.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.HttpServer.Configuration;

namespace Silky.Lms.HttpServer
{
    public class HttpServerLmsStartup : ILmsStartup
    {
        private const string MiniProfilerRouteBasePath = "/index-mini-profiler";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<GatewayOptions>()
                .Bind(configuration.GetSection(GatewayOptions.Gateway));
            var injectMiniProfiler = configuration.GetValue<bool?>("appSettings:injectMiniProfiler") ?? false;
            if (injectMiniProfiler)
            {
                services.AddMiniProfiler(options =>
                {
                    options.RouteBasePath = MiniProfilerRouteBasePath;
                    // Optionally use something other than the "light" color scheme.
                    options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                    // Enabled sending the Server-Timing header on responses
                    options.EnableServerTimingHeader = true;
                    options.IgnoredPaths.Add("/lib");
                    options.IgnoredPaths.Add("/css");
                    options.IgnoredPaths.Add("/js");
                    options.IgnoredPaths.Add("/swagger");
                    
                });
            }
        }

        public void Configure(IApplicationBuilder application)
        {
            var injectMiniProfiler = EngineContext.Current.Configuration.GetValue<bool?>("appSettings:injectMiniProfiler") ?? false;
            if (injectMiniProfiler)
            {
                application.UseMiniProfiler();
            }
            application.UseLmsExceptionHandler();
            application.UseLms();
            
        }

        public int Order { get; } = Int32.MinValue;
    }
}