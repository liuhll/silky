using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Monitor.Handle;
using Silky.Rpc.Monitor.Invoke;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RpcMonitorCollectionExtensions
    {
        public static IServiceCollection AddRpcSupervisor(this IServiceCollection services)
        {
            services.TryAdd(ServiceDescriptor.Scoped<IServerHandleMonitor, DefaultServerHandleMonitor>());
            services.TryAdd(ServiceDescriptor.Scoped<IInvokeMonitor, DefaultInvokeMonitor>());
            return services;
        }
    }
}