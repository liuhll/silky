using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.Rpc;

namespace Silky.RegistryCenter.Nacos
{
    [DependsOn(typeof(RpcModule))]
    public class NacosModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNacosRegistryCenter(NacosRegistryCenterOptions.RegistryCenterSection);
        }
    }
}