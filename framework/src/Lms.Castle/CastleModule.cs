using Autofac;
using Lms.Castle.Adapter;
using Lms.Core.Modularity;

namespace Lms.Castle
{
    public class CastleModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(LmsAsyncDeterminationInterceptor<>))
                .PropertiesAutowired()
                .AsImplementedInterfaces()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}