using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Modularity;
using Silky.Http.Swagger;
using Silky.Http.Swagger.Configuration;
using Silky.Rpc.MiniProfiler;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Silky.Http.MiniProfiler
{
    [DependsOn(typeof(SwaggerModule))]
    public class MiniProfilerModule : WebSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = MiniProfileConstant.MiniProfilerRouteBasePath;
                // Optionally use something other than the "light" color scheme.
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                // Enabled sending the Server-Timing header on responses
                options.EnableServerTimingHeader = true;
                options.IgnoredPaths.Add("/lib");
                options.IgnoredPaths.Add("/css");
                options.IgnoredPaths.Add("/js");
                options.IgnoredPaths.Add("/swagger");
            });
            services.AddTransient<IMiniProfiler, DefaultMiniProfiler>();
        }

        public override void Configure(IApplicationBuilder application)
        {
            SwaggerDocumentOptions swaggerDocumentOptions = default;
            swaggerDocumentOptions = EngineContext.Current.GetOptionsMonitor<SwaggerDocumentOptions>(
                (options, name) => { swaggerDocumentOptions = options; });

            application.UseMiniProfiler();
        }
    }
}