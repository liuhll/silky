using Autofac;
using Silky.Castle;
using Silky.Core.Modularity;
using Silky.Rpc.Runtime;

namespace Silky.Rpc.CachingInterceptor
{
    [DependsOn(typeof(RpcModule))]
    public class RpcCachingInterceptorModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultServiceExecutor>()
                .As<IServiceExecutor>()
                .InstancePerLifetimeScope()
                .AddInterceptors(
                    typeof(CachingInterceptor)
                )
                ;
        }
    }
}