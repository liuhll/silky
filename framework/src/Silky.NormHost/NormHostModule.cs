using Silky.AutoMapper;
using Silky.Codec;
using Silky.Core.Modularity;
using Silky.DotNetty.Protocol.Tcp;
using Silky.RegistryCenter.Zookeeper;
using Silky.Rpc.Proxy;
using Silky.Transaction.Tcc;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(MessagePackModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(AutoMapperModule)
    )]
    public class NormHostModule : StartUpModule
    {
    }
}