using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Http.Swagger;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Silky.Http.MiniProfiler
{
    [DependsOn(typeof(SwaggerModule))]
    public class MiniProfilerModule : HttpSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkyMiniProfiler();
        }

        public override void Configure(IApplicationBuilder application)
        {
            application.UseMiniProfiler();
        }
    }
}