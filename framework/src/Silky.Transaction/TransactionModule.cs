using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Castle;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Configuration;
using Silky.Transaction.Interceptors;
using Silky.Transaction.Schedule;

namespace Silky.Transaction
{
    [DependsOn(typeof(RpcModule))]
    public class TransactionModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DistributedTransactionOptions>()
                .Bind(configuration.GetSection(DistributedTransactionOptions.DistributedTransaction));
            services.AddHostedService<TransactionSelfRecoveryScheduled>();
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultLocalExecutor>()
                .As<ILocalExecutor>()
                .InstancePerLifetimeScope()
                .AddInterceptors(
                    typeof(TransactionInterceptor)
                )
                ;
        }
    }
}