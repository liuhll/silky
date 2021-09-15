using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.DotNetty;
using Silky.Validation.Fluent;
using Silky.Http.Core;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Proxy;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(RpcProxyModule),
        typeof(ZookeeperModule),
        typeof(SilkyHttpCoreModule),
        typeof(RpcCachingInterceptorModule),
        typeof(DotNettyModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule)
    )]
    public abstract class WebHostModule : StartUpModule
    {
    }
}