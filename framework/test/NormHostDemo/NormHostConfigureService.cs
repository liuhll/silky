using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NormHostDemo.Contexts;
using Silky.Lms.Core;

namespace NormHostDemo
{
    public class NormHostConfigureService : IConfigureService
    {
        private const string MiniProfilerRouteBasePath = "/index-mini-profiler";
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseAccessor(options =>
            {
                options.AddDbPool<DemoDbContext>();
            },"NormHostDemo");
            var injectMiniProfiler = configuration.GetValue<bool?>("appSettings:injectMiniProfiler") ?? false;
            if (injectMiniProfiler)
            {
                services.AddMiniProfiler(options => { options.RouteBasePath = MiniProfilerRouteBasePath; });
            }
        }

        public int Order { get; } = 10;
    }
}