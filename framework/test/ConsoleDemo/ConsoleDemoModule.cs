using Autofac;
using Lms.Core.Modularity;
using Lms.RegistryCenter.Zookeeper;
using Lms.Rpc;

namespace ConsoleDemo
{
    [DependsOn(typeof(RpcModule),typeof(ZookeeperModule))]
    public class ConsoleDemoModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
        }
    }
}