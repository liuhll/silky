using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching;
using Silky.Core.Modularity;

namespace Silky.Rpc.Monitor
{
    [DependsOn(typeof(RpcModule), typeof(CachingModule))]
    public class RpcMonitorModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRpcSupervisor();
        }
    }
}