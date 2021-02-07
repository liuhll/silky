using Autofac;
using Lms.Core.Modularity;
using Lms.RegistryCenter.Zookeeper;
using Lms.Rpc;

namespace NormHostDemo
{
    [DependsOn(typeof(RpcModule),typeof(ZookeeperModule))]
    public class NormDemoModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
        }
    }
}