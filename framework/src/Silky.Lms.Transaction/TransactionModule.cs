using Autofac;
using Silky.Lms.Castle;
using Silky.Lms.Core.Modularity;
using Silky.Lms.Rpc;
using Silky.Lms.Rpc.Runtime.Client;
using Silky.Lms.Rpc.Runtime.Server;
using Silky.Lms.Transaction.Interceptors;

namespace Silky.Lms.Transaction
{
    [DependsOn(typeof(RpcModule))]
    public class TransactionModule : LmsModule
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