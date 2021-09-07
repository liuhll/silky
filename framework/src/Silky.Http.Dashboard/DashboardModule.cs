using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Modularity;
using Silky.Http.Dashboard.Configuration;
using Silky.Http.Dashboard.Middlewares;

namespace Silky.Http.Dashboard
{
    public class DashboardModule : WebSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DashboardOptions>()
                .Bind(configuration.GetSection(DashboardOptions.Dashboard));
        }

        public override void Configure(IApplicationBuilder application)
        {
            var dashboardOptions = EngineContext.Current.GetOptions<DashboardOptions>();
            if (dashboardOptions.UseAuth && dashboardOptions.LoginWebApi.IsNullOrEmpty())
            {
                throw new SilkyException("Dashboard requires authentication, please set the login webapi");
            }
            application.UseMiddleware<UiMiddleware>();
        }
    }
}