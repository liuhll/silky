using Autofac;
using Lms.Core.Modularity;

namespace Lms.Codec
{
    public class ProtoBufferModule : LmsModule
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