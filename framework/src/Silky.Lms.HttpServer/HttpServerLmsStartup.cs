using System;
using Silky.Lms.Core;
using Silky.Lms.HttpServer.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.HttpServer.Configuration;
using Silky.Lms.HttpServer.SwaggerDocument;
using Silky.Lms.Rpc;

namespace Silky.Lms.HttpServer
{
    public class HttpServerLmsStartup : ILmsStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<GatewayOptions>()
                .Bind(configuration.GetSection(GatewayOptions.Gateway));
            services.AddOptions<SwaggerDocumentOptions>()
                .Bind(configuration.GetSection(SwaggerDocumentOptions.SwaggerDocument));

            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = MiniProfilerConstants.MiniProfilerRouteBasePath;
                // Optionally use something other than the "light" color scheme.
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                // Enabled sending the Server-Timing header on responses
                options.EnableServerTimingHeader = true;
                options.IgnoredPaths.Add("/lib");
                options.IgnoredPaths.Add("/css");
                options.IgnoredPaths.Add("/js");
                options.IgnoredPaths.Add("/swagger");
            });
            services.AddSwaggerDocuments(configuration);
        }

        public void Configure(IApplicationBuilder application)
        {
            var gatewayOption = EngineContext.Current.GetOptions<GatewayOptions>();

            var swaggerDocumentOptions = EngineContext.Current.GetOptions<SwaggerDocumentOptions>();

            if (gatewayOption.EnableSwaggerDoc)
            {
                application.UseSwaggerDocuments(swaggerDocumentOptions);
            }

            if (swaggerDocumentOptions.InjectMiniProfiler)
            {
                application.UseMiniProfiler();
            }

            application.UseLmsExceptionHandler();
            application.UseLms();
        }

        public int Order { get; } = Int32.MinValue;
    }
}