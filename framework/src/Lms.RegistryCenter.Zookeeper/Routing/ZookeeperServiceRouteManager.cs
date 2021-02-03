using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Routing;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry;

namespace Lms.RegistryCenter.Zookeeper.Routing
{
    public class ZookeeperServiceRouteManager : ServiceRouteManagerBase, ISingletonDependency
    {
        private readonly IZookeeperClientProvider _zookeeperClientProvider;

        public ZookeeperServiceRouteManager(ServiceRouteCache serviceRouteCache,
            IServiceEntryManager serviceEntryManager,
            IZookeeperClientProvider zookeeperClientProvider)
            : base(serviceRouteCache, serviceEntryManager)
        {
            _zookeeperClientProvider = zookeeperClientProvider;
            EnterRoutes().GetAwaiter().GetResult();
        }

        protected async Task EnterRoutes()
        {
            var zookeeperClient = _zookeeperClientProvider.GetZooKeeperClient();
        }

        protected async override Task SetRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor)
        {
        }
    }
}