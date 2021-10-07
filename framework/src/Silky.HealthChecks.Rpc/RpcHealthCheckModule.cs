using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;

namespace Silky.HealthChecks.Rpc
{
    public class RpcHealthCheckModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkyServerHealthCheck();
        }
    }
}