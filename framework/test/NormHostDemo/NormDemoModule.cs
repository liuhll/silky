using Autofac;
using Lms.Codec;
using Lms.Core.Modularity;
using Lms.DotNetty.Protocol.Tcp;
using Lms.RegistryCenter.Zookeeper;

namespace NormHostDemo
{
    [DependsOn(typeof(ZookeeperModule), typeof(DotNettyTcpModule), typeof(MessagePackModule))]
    public class NormDemoModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
        }
    }
}