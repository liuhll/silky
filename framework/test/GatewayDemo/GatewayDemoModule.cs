using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Modularity;
using Lms.DotNetty;
using Lms.HttpServer;
using Lms.RegistryCenter.Zookeeper;
using Lms.Rpc;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime.Server;
using Microsoft.Extensions.Options;

namespace GatewayDemo
{
    [DependsOn(typeof(RpcModule),
        typeof(ZookeeperModule),
        typeof(HttpServerModule),
        typeof(DotNettyModule))]
    public class GatewayDemoModule : LmsModule
    {
        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var serviceEntryManager = EngineContext.Current.Resolve<IServiceEntryManager>();
            var entries = serviceEntryManager.GetAllEntries();
            var options = EngineContext.Current.Resolve<IOptions<RegistryCenterOptions>>();
        }
    }
}