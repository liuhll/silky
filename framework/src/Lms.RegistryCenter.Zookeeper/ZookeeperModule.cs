using Autofac;
using Lms.Core.Modularity;
using Lms.RegistryCenter.Zookeeper.Routing;
using Lms.Rpc.Routing;

namespace Lms.RegistryCenter.Zookeeper
{
    public class ZookeeperModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<ZookeeperServiceRouteManager>().As<IServiceRouteManager>().SingleInstance();
        }
    }
}