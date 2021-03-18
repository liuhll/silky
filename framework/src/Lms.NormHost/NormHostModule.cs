using Lms.AutoMapper;
using Lms.Codec;
using Lms.Core.Modularity;
using Lms.DotNetty.Protocol.Tcp;
using Lms.RegistryCenter.Zookeeper;
using Lms.Rpc.Proxy;
using Lms.Transaction.Tcc;

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