using Autofac;
using Lms.Core.Modularity;

namespace Lms.Codec
{
    public class MessagePackModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<MessagePackTransportMessageDecoder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
            builder.RegisterType<MessagePackTransportMessageEncoder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
        }
    }
}