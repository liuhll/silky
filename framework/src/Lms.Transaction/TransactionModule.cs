using Autofac;
using Lms.Castle;
using Lms.Core.Modularity;
using Lms.Rpc;
using Lms.Rpc.Runtime.Client;
using Lms.Rpc.Runtime.Server;
using Lms.Transaction.Interceptors;

namespace Lms.Transaction
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