using System;
using Silky.Core;
using Silky.HttpServer.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.HttpServer.Configuration;
using Silky.HttpServer.SwaggerDocument;
using Silky.Rpc;

namespace Silky.HttpServer
{
    public class HttpServerSilkyStartup : ISilkyStartup
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

            application.UseSilkyExceptionHandler();
            application.UseSilky();
        }

        public int Order { get; } = Int32.MinValue;
    }
}