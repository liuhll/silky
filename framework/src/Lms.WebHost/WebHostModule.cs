using Lms.Codec;
using Lms.Core.Modularity;
using Lms.DotNetty;
using Lms.HttpServer;
using Lms.RegistryCenter.Zookeeper;
using Lms.Rpc.Proxy;
using Lms.Transaction.Tcc;

namespace Microsoft.Extensions.Hosting
{    [DependsOn(typeof(RpcProxyModule),
        typeof(ZookeeperModule),
        typeof(HttpServerModule),
        typeof(DotNettyModule),
        typeof(MessagePackModule),
        typeof(TransactionTccModule)
    )]
    public class WebHostModule : LmsModule
    {
        
    }
}