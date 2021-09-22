using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;
using Silky.Rpc;
using Silky.Lock;
using Silky.RegistryCenter.Zookeeper.Server;

namespace Silky.RegistryCenter.Zookeeper
{
    [DependsOn(typeof(RpcModule), typeof(LockModule))]
    public class ZookeeperModule : SilkyModule
    {
        public override async Task Shutdown(ApplicationContext applicationContext)
        {
            var zookeeperServerRouteManager =
                applicationContext.ServiceProvider.GetRequiredService<ZookeeperServerRegister>();
            await zookeeperServerRouteManager.RemoveLocalHostServiceRoute();
        }
    }
}