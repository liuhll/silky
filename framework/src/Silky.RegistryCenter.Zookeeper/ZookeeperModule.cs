using System.Threading.Tasks;
using Silky.Core.Modularity;
using Silky.Rpc;
using Microsoft.Extensions.DependencyInjection;
using Silky.RegistryCenter.Zookeeper.Routing;

namespace Silky.RegistryCenter.Zookeeper
{
    [DependsOn(typeof(RpcModule))]
    public class ZookeeperModule : SilkyModule
    {
        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var serviceRouteManager = applicationContext.ServiceProvider.GetRequiredService<ZookeeperServiceRouteManager>();
            if (serviceRouteManager != null)
            {
                await serviceRouteManager.RemoveLocalHostServiceRoute();
            }
           
        }
    }
}