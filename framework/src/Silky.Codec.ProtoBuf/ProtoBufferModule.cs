using Autofac;
using Silky.Core.Modularity;
using Silky.Rpc;

namespace Silky.Codec
{
    [DependsOn(typeof(RpcModule))]
    public class ProtoBufferModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<ProtoBufferTransportMessageDecoder>().AsSelf().AsImplementedInterfaces()
                .InstancePerDependency();
            builder.RegisterType<ProtoBufferTransportMessageEncoder>().AsSelf().AsImplementedInterfaces()
                .InstancePerDependency();
        }
    }
}