using Silky.Lms.Codec;
using Silky.Lms.Core.Modularity;
using Silky.Lms.DotNetty;
using Silky.Lms.HttpServer;
using Silky.Lms.RegistryCenter.Zookeeper;
using Silky.Lms.Rpc.Proxy;
using Silky.Lms.Transaction.Tcc;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(RpcProxyModule),
       typeof(ZookeeperModule),
       typeof(HttpServerModule),
       typeof(DotNettyModule),
       typeof(MessagePackModule),
       typeof(TransactionTccModule)
   )]
    public class WebHostModule : StartUpModule
    {

    }
}