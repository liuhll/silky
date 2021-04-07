using Autofac;
using Silky.Lms.Core.Modularity;

namespace Silky.Lms.Codec
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