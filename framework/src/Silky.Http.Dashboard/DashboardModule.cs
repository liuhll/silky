using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Http.Core;

namespace Silky.Http.Dashboard
{
    [DependsOn(typeof(SilkyHttpCoreModule))]
    public class DashboardModule : WebSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDashboard();
        }

        public override void Configure(IApplicationBuilder application)
        {
            application.UseDashboard();
        }
    }
}