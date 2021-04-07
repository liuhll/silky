using Silky.Lms.AutoMapper;
using Silky.Lms.Codec;
using Silky.Lms.Core.Modularity;
using Silky.Lms.DotNetty.Protocol.Tcp;
using Silky.Lms.RegistryCenter.Zookeeper;
using Silky.Lms.Rpc.Proxy;
using Silky.Lms.Transaction.Tcc;

namespace Microsoft.Extensions.Hosting
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(MessagePackModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(AutoMapperModule)
    )]
    public class NormHostModule : LmsModule
    {
    }
}