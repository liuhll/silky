using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Lock;
using Silky.RegistryCenter.Zookeeper.Configuration;

namespace Silky.RegistryCenter.Zookeeper
{
    [DependsOn(typeof(RpcModule), typeof(LockModule))]
    public class ZookeeperModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ZookeeperRegistryCenterOptions>()
                .Bind(configuration.GetSection(ZookeeperRegistryCenterOptions.RegistryCenter));;
        }

        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var zookeeperServerRouteManager =
                applicationContext.ServiceProvider.GetRequiredService<ZookeeperServerRegister>();
            await zookeeperServerRouteManager.RemoveLocalHostServiceRoute();
        }
    }
}