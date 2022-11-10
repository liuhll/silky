using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching;
using Silky.Core.Modularity;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Monitor
{
    [DependsOn(typeof(RpcModule), typeof(CachingModule))]
    public class RpcMonitorModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRpcSupervisor();
        }

        public override async Task Initialize(ApplicationInitializationContext context)
        {
            await context.ServiceProvider.GetRequiredService<IServerHandleMonitor>().ClearCache();
            await context.ServiceProvider.GetRequiredService<IInvokeMonitor>().ClearCache();
        }

        public override async Task Shutdown(ApplicationShutdownContext context)
        {
            await context.ServiceProvider.GetRequiredService<IServerHandleMonitor>().ClearCache();
            await context.ServiceProvider.GetRequiredService<IInvokeMonitor>().ClearCache();
        }
    }
}