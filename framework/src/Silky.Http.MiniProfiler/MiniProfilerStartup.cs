using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Core.Configuration;
using Silky.Rpc.MiniProfiler;

namespace Silky.Http.MiniProfiler
{
    public class MiniProfilerStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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

        public int Order { get; } = 1;

        public void Configure(IApplicationBuilder application)
        {
            var swaggerDocumentOptions = EngineContext.Current.GetOptions<SwaggerDocumentOptions>();
            if (swaggerDocumentOptions.InjectMiniProfiler)
            {
                application.UseMiniProfiler();
            }
        }
    }
}