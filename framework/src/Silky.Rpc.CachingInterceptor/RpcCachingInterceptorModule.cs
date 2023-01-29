using Autofac;
using Silky.Caching;
using Silky.Castle;
using Silky.Core.Modularity;
using Silky.Rpc.Runtime;

namespace Silky.Rpc.CachingInterceptor
{
    [DependsOn(typeof(RpcModule), typeof(CachingModule))]
    public class RpcCachingInterceptorModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultExecutor>()
                .As<IExecutor>()
                .InstancePerDependency()
                .AddInterceptors(
                    typeof(CachingInterceptor)
                )
                ;
        }
    }
}