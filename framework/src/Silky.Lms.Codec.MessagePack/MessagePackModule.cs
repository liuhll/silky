using Autofac;
using Silky.Lms.Core.Modularity;

namespace Silky.Lms.Codec
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