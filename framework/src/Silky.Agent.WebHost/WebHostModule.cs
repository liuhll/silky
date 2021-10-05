using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;
using Silky.DotNetty;
using Silky.DotNetty.Protocol.Tcp;
using Silky.Validation.Fluent;
using Silky.Http.Core;
using Silky.Http.CorsAccessor;
using Silky.Http.Identity;
using Silky.Http.MiniProfiler;
using Silky.Http.RateLimit;
using Silky.Http.Swagger;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Proxy;
using Silky.Rpc.Runtime.Server;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(SilkyHttpCoreModule),
        typeof(SwaggerModule),
        typeof(MiniProfilerModule),
        typeof(RateLimitModule),
        typeof(IdentityModule),
        typeof(CorsModule),
        typeof(RpcCachingInterceptorModule),
        typeof(DotNettyModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule)
    )]
    public abstract class WebHostModule : StartUpModule
    {
        public override async Task Initialize(ApplicationContext applicationContext)
        {
            var serverRouteRegister =
                applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
            await serverRouteRegister.RegisterServer();
        }

        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var serverRegister =
                applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
            await serverRegister.RemoveSelf();
        }
    }
}