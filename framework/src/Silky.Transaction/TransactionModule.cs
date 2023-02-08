using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Medallion.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching;
using Silky.Castle;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;
using Silky.DistributedLock.Redis;
using Silky.Rpc;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Configuration;
using Silky.Transaction.Interceptor;
using Silky.Transaction.Schedule;

namespace Silky.Transaction
{
    [DependsOn(typeof(RpcModule), typeof(CachingModule))]
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

        public override async Task PreInitialize(ApplicationInitializationContext context)
        {
            if (!context.ServiceProvider.GetAutofacRoot().IsRegistered(typeof(IRedisDistributedLockProvider)))
            { 
                throw new SilkyException(
                    "You must specify the implementation of IDistributedLockProvider in the Transaction.Repository project of the distributed transaction");
            }
        }
    }
}