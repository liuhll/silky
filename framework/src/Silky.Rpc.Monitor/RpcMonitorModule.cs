using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Caching;
using Silky.Core.Modularity;
using Silky.Rpc.Configuration;
using Silky.Rpc.Monitor.Provider;
using IServiceProvider = System.IServiceProvider;


namespace Silky.Rpc.Monitor
{
    [DependsOn(typeof(RpcModule), typeof(CachingModule))]
    public class RpcMonitorModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var rpcOption = configuration.GetSection("rpc").Get<RpcOptions>();
            if (rpcOption?.EnableMonitor == true)
            {
                services.AddRpcSupervisor();
            }
        }

        public override async Task PostInitialize(ApplicationInitializationContext context)
        {
            await CleanMonitorCache(context.ServiceProvider);
        }

        public override async Task Shutdown(ApplicationShutdownContext context)
        {
            await CleanMonitorCache(context.ServiceProvider);
        }

        private async Task CleanMonitorCache(IServiceProvider serviceProvider)
        {
            var rpcOption = serviceProvider.GetRequiredService<IOptions<RpcOptions>>();
           
            if (rpcOption.Value?.EnableMonitor == true)
            {
                var monitorProvider = serviceProvider.GetRequiredService<IMonitorProvider>();
                await monitorProvider.ClearClientInvokeCache();
                await monitorProvider.ClearServerHandleCache();
            }
        }
    }
}