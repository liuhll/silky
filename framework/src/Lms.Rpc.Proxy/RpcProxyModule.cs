using Autofac;
using Lms.Castle;
using Lms.Core.Modularity;
using Lms.Rpc.Proxy.Interceptors;
using Lms.Rpc.Runtime;

namespace Lms.Rpc.Proxy
{
    [DependsOn(typeof(RpcModule), typeof(CastleModule))]
    public class RpcProxyModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultServiceExecutor>()
                .As<IServiceExecutor>()
                .InstancePerLifetimeScope()
                .AddInterceptors(typeof(CachingInterceptor));
        }
    }
}