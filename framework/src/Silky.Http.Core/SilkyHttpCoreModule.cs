using Autofac;
using Silky.Castle;
using Silky.Core.Modularity;
using Silky.Http.Core.Executor;
using Silky.Rpc;
using Silky.Transaction.Interceptor;

namespace Silky.Http.Core
{
    [DependsOn(typeof(RpcModule))]
    public class SilkyHttpCoreModule : HttpSilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultHttpExecutor>()
                .As<IHttpExecutor>()
                .InstancePerLifetimeScope()
                ;
        }
    }
}