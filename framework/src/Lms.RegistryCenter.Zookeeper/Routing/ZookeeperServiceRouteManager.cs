using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Routing;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry;

namespace Lms.RegistryCenter.Zookeeper.Routing
{
    public class ZookeeperServiceRouteManager : ServiceRouteManagerBase
    {
        public ZookeeperServiceRouteManager(ServiceRouteCache serviceRouteCache, 
            IServiceEntryManager serviceEntryManager) 
            : base(serviceRouteCache, serviceEntryManager)
        {
        }

        protected async override Task EnterRoutes()
        {
            
        }

        protected async override Task SetRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor)
        {
            
        }
    }
}