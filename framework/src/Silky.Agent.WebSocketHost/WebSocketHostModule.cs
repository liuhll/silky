using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.WebSocket;
using Silky.DotNetty.Protocol.Tcp;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Proxy;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;
using Silky.Validation;
using Silky.Validation.Fluent;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(RpcCachingInterceptorModule),
        typeof(TransactionTccModule),
        typeof(WebSocketModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(RedisTransactionRepositoryModule)
    )]
    public class WebSocketHostModule : StartUpModule
    {
    }
}