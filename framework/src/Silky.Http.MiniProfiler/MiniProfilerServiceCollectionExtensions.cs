using Silky.Core.MiniProfiler;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MiniProfilerServiceCollectionExtensions
    {
        public static IServiceCollection AddSilkyMiniProfiler(
            this IServiceCollection services)
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
            return services;
        }
    }
}