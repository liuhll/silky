using System.Threading.Tasks;
using Lms.Core.Modularity;
using Lms.RegistryCenter.Zookeeper.Routing;
using Lms.Rpc;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.RegistryCenter.Zookeeper
{
    [DependsOn(typeof(RpcModule))]
    public class ZookeeperModule : LmsModule
    {
        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var serviceRouteManager = applicationContext.ServiceProvider.GetService<ZookeeperServiceRouteManager>();
            if (serviceRouteManager != null)
            {
                await serviceRouteManager.RemoveLocalHostServiceRoute();
            }
           
        }
    }
}