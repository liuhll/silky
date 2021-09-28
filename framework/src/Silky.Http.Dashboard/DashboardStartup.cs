using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Http.Dashboard
{
    public class DashboardStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDashboard();
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseDashboard();
        }

        public int Order { get; } = -1;
    }
}