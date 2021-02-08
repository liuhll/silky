using Autofac;
using Lms.Core.Modularity;
using Lms.DotNetty.Tcp;
using Lms.RegistryCenter.Zookeeper;
using Lms.Rpc;

namespace NormHostDemo
{
    [DependsOn(typeof(DotnetTcpModule),typeof(ZookeeperModule))]
    public class NormDemoModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
        }
    }
}