using System.Threading.Tasks;
using Silky.Lms.Core.Modularity;
using Silky.Lms.Rpc;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.RegistryCenter.Zookeeper.Routing;

namespace Silky.Lms.RegistryCenter.Zookeeper
{
    [DependsOn(typeof(RpcModule))]
    public class ZookeeperModule : LmsModule
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