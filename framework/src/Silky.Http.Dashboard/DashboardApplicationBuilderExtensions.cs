using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Http.Dashboard.Configuration;
using Silky.Http.Dashboard.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class DashboardApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDashboard(this IApplicationBuilder application)
        {
            var dashboardOptions = EngineContext.Current.GetOptions<DashboardOptions>();
            if (dashboardOptions.UseAuth && dashboardOptions.DashboardLoginApi.IsNullOrEmpty())
            {
                throw new SilkyException("Dashboard requires authentication, please set the login webapi");
            }

            application.UseMiddleware<UiMiddleware>();
            return application;
        }
    }
}