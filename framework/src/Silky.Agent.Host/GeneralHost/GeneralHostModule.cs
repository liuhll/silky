using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.DotNetty.Protocol.Tcp;
using Silky.Validation.Fluent;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Proxy;
using Silky.Swagger.Gen;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        // typeof(RpcMonitorModule),
        typeof(RpcCachingInterceptorModule),
        typeof(TransactionTccModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(RedisTransactionRepositoryModule),
        typeof(SwaggerGenModule)
    )]
    public abstract class GeneralHostModule : HostAgentModule
    {
        
    }
}