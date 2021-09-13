using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Castle;
using Silky.Core.Modularity;

namespace Silky.Rpc.Proxy
{
    [DependsOn(typeof(RpcModule), typeof(CastleModule))]
    public class RpcProxyModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRpcProxy();
        }
    }
}