using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.DotNetty.Protocol.Tcp;
using Silky.Validation.Fluent;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.Proxy;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(TransactionRepositoryRedisModule)
    )]
    public abstract class GeneralHostModule : StartUpModule
    {
    }
}