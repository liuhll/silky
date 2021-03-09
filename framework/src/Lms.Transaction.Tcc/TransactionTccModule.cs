using Autofac;
using Lms.Castle;
using Lms.Core.Modularity;
using Lms.Rpc.Runtime.Server;
using Lms.Transaction.Tcc.Interceptor;

namespace Lms.Transaction.Tcc
{
    public class TransactionTccModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            RegisterServicesForServiceExecutor(builder);
        }
        
        private void RegisterServicesForServiceExecutor(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalExecutor>()
                .As<ILocalExecutor>()
                .InstancePerLifetimeScope()
                .AddInterceptors(
                    typeof(TransactionInterceptor))
                ;
        }
    }
}