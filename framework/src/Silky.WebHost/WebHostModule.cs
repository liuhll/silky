using Silky.Codec;
using Silky.Core.Modularity;
using Silky.DotNetty;
using Silky.HttpServer;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.Proxy;
using Silky.Transaction.Tcc;

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