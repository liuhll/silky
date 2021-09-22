using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.DotNetty.Protocol.Tcp;
using Silky.Validation.Fluent;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Proxy;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Repository.Redis;
using Silky.Transaction.Tcc;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(RpcCachingInterceptorModule),
        typeof(TransactionTccModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(RedisTransactionRepositoryModule)
    )]
    public abstract class GeneralHostModule : StartUpModule
    {
        public override async Task Initialize(ApplicationContext applicationContext)
        {
            var serverRouteRegister =
                applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
            await serverRouteRegister.RegisterServer();
        }
    }
}