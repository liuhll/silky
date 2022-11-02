using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.DotNetty;
using Silky.Http.Core;
using Silky.Http.CorsAccessor;
using Silky.Http.MiniProfiler;
using Silky.Http.RateLimit;
using Silky.Http.Swagger;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Proxy;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;
using Silky.Validation;
using Silky.Validation.Fluent;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(
        typeof(RpcProxyModule),
        // typeof(RpcMonitorModule),
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
    public abstract class GatewayHostModule : HostAgentModule
    {
       
        
    }
}