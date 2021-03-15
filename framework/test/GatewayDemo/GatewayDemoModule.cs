using Lms.Codec;
using Lms.Core.Modularity;
using Lms.DotNetty;
using Lms.HttpServer;
using Lms.RegistryCenter.Zookeeper;
using Lms.Rpc.Proxy;

namespace GatewayDemo
{
    [DependsOn(typeof(RpcProxyModule),
        typeof(ZookeeperModule),
        typeof(HttpServerModule),
        typeof(DotNettyModule),
        typeof(MessagePackModule)
        )]
    public class GatewayDemoModule : LmsModule
    {
    }
}