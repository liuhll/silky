using Autofac;
using Silky.Core.Modularity;

namespace Silky.Codec
{
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