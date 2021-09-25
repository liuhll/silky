using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc;

namespace Silky.Http.Core
{
    [DependsOn(typeof(RpcModule))]
    public class SilkyHttpCoreModule : HttpSilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public override void Configure(IApplicationBuilder application)
        {
        }
    }
}