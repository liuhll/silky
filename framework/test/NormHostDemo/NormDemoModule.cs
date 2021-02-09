using Autofac;
using Lms.Core.Modularity;
using Lms.DotNetty.Protocol.Tcp;
using Lms.RegistryCenter.Zookeeper;

namespace NormHostDemo
{
    [DependsOn(typeof(ZookeeperModule),typeof(DotNettyTcpModule))]
    public class NormDemoModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
        }
    }
}