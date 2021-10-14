using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.StackExchangeRedis;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Modularity;
using Silky.DotNetty;
using Silky.Http.Core;
using Silky.Http.CorsAccessor;
using Silky.Http.Identity;
using Silky.Http.MiniProfiler;
using Silky.Http.RateLimit;
using Silky.Http.Swagger;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Proxy;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Monitor;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;
using Silky.Validation;
using Silky.Validation.Fluent;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(
        typeof(RpcProxyModule),
        typeof(RpcMonitorModule),
        typeof(SilkyHttpCoreModule),
        typeof(SwaggerModule),
        typeof(MiniProfilerModule),
        typeof(RateLimitModule),
        typeof(CorsModule),
        typeof(RpcCachingInterceptorModule),
        typeof(DotNettyModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(TransactionTccModule),
        typeof(RedisTransactionRepositoryModule)
    )]
    public abstract class GatewayHostModule : StartUpModule
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

        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var serverRegister =
                applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
            await serverRegister.RemoveSelf();
        }
    }
}