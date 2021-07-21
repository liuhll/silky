using Silky.Core.Modularity;
using Silky.DotNetty.Protocol.Tcp;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.Proxy;
using Silky.Transaction.Tcc;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule)
    )]
    public abstract class GeneralHostModule : StartUpModule
    {
    }
}