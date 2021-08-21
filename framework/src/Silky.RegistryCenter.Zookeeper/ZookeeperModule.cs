using System.Threading.Tasks;
using Silky.Core.Modularity;
using Silky.Rpc;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lock;
using Silky.RegistryCenter.Zookeeper.Gateway;
using Silky.RegistryCenter.Zookeeper.Routing;

namespace Silky.RegistryCenter.Zookeeper
{
    [DependsOn(typeof(RpcModule), typeof(LockModule))]
    public class ZookeeperModule : SilkyModule
    {
        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var serviceRouteManager =
                applicationContext.ServiceProvider.GetRequiredService<ZookeeperServiceRouteManager>();
            var gatewayManger =
                applicationContext.ServiceProvider.GetRequiredService<ZookeeperGatewayManager>();
            if (serviceRouteManager != null)
            {
                await serviceRouteManager.RemoveLocalHostServiceRoute();
                await gatewayManger.RemoveLocalGatewayAddress();
            }
        }
    }
}