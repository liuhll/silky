using Silky.Core;
using Silky.Http.Dashboard.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DashboardServiceCollectionExtensions
    {
        public static IServiceCollection AddDashboard(this IServiceCollection services)
        {
            services.AddOptions<DashboardOptions>()
                .Bind(EngineContext.Current.Configuration.GetSection(DashboardOptions.Dashboard));
            services.AddSilkyServerHealthCheck();
            return services;
        }
    }
}