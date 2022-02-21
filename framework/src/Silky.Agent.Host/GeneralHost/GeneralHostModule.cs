using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.Modularity;
using Silky.DotNetty.Protocol.Tcp;
using Silky.Validation.Fluent;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Configuration;
using Silky.Rpc.Proxy;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Monitor;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(RpcMonitorModule),
        typeof(RpcCachingInterceptorModule),
        typeof(TransactionTccModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(RedisTransactionRepositoryModule)
    )]
    public abstract class GeneralHostModule : StartUpModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (!services.IsAdded(typeof(IServerRegister)))
            {
                var registerType = configuration.GetValue<string>("registrycenter:type");
                if (registerType.IsNullOrEmpty())
                {
                    throw new SilkyException("You did not specify the service registry type");
                }

                services.AddDefaultRegistryCenter(registerType);
            }
        }

        public override async Task Initialize(ApplicationContext applicationContext)
        {
            var options = EngineContext.Current.GetOptions<RpcOptions>();
            var logger = EngineContext.Current.Resolve<ILogger<GeneralHostModule>>();
            var serverRouteRegister =
                applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
            var policy = Policy
                .Handle<TimeoutException>()
                .WaitAndRetryAsync(options.RegisterFailureRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    async (exception, timeSpan, context) =>
                    {
                        if (exception != null)
                        {
                            logger.LogException(exception);
                        }

                        await serverRouteRegister.RegisterServer();
                    });
            await policy.ExecuteAsync(async () => { await serverRouteRegister.RegisterServer(); });
        }

        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var serverRegister =
                applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
            await serverRegister.RemoveSelf();
        }
    }
}