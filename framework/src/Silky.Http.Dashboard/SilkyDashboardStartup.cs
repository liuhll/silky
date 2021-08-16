using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Http.Dashboard.Middlewares;

namespace Silky.Http.Dashboard
{
    public class SilkyDashboardStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            
        }

        public int Order { get; } = 1;
        public void Configure(IApplicationBuilder application)
        {
            application.UseMiddleware<UiMiddleware>();
        }
    }
}