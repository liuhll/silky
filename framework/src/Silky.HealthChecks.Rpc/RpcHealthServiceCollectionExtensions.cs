using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.HealthChecks.Rpc.ServerCheck;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RpcHealthServiceCollectionExtensions
    {
        public static IServiceCollection AddSilkyServerHealthCheck(this IServiceCollection services)
        {
            services.TryAdd(ServiceDescriptor.Transient<IServerHealthCheck, DefaultServerHealthCheck>());
            return services;
        }
    }
}