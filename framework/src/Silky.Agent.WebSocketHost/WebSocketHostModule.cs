using Silky.Core.Modularity;
using Silky.WebSocket;
using Silky.DotNetty.Protocol.Tcp;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.Proxy;
using Silky.Transaction.Tcc;

namespace Silky.Agent.WebSocketHost
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(WebSocketModule))]
    public abstract class WebSocketHostModule : StartUpModule
    {
    }
}