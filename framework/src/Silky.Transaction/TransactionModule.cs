using Autofac;
using Silky.Castle;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Interceptors;

namespace Silky.Transaction
{
    [DependsOn(typeof(RpcModule))]
    public class TransactionModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalExecutor>()
                .As<ILocalExecutor>()
                .InstancePerLifetimeScope()
                .AddInterceptors(
                    typeof(TransactionInterceptor)
                )
                ;

            builder.RegisterType<DefaultRemoteServiceExecutor>()
                .As<IRemoteServiceExecutor>()
                .InstancePerLifetimeScope()
                .AddInterceptors(
                    typeof(TransactionInterceptor)
                )
                ;
        }
    }
}