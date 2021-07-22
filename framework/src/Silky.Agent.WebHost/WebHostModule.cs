using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.DotNetty;
using Silky.FluentValidation;
using Silky.HttpServer;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.Proxy;
using Silky.Transaction.Tcc;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(RpcProxyModule),
       typeof(ZookeeperModule),
       typeof(HttpServerModule),
       typeof(DotNettyModule),
       typeof(TransactionTccModule),
       typeof(ValidationModule),
       typeof(FluentValidationModule),
       typeof(RedisCachingModule)
   )]
    public abstract class WebHostModule : StartUpModule
    {

    }
}