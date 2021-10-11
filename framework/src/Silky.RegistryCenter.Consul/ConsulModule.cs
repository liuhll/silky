using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc;

namespace Silky.RegistryCenter.Consul
{
    [DependsOn(typeof(RpcModule))]
    public class ConsulModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddConsulRegistryCenter(ConsulRegistryCenterOptions.RegistryCenterSection);
        }
    }
}