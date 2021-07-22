using Silky.Core.Modularity;
using Silky.WebSocket;
using Silky.DotNetty.Protocol.Tcp;
using Silky.FluentValidation;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.Proxy;
using Silky.Transaction.Tcc;
using Silky.Validation;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(WebSocketModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule)
        )]
    public abstract class WebSocketHostModule : StartUpModule
    {
    }
}