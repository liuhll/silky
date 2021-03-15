using Lms.Codec;
using Lms.Core.Modularity;
using Lms.DotNetty;
using Lms.DotNetty.Protocol.Tcp;
using Lms.RegistryCenter.Zookeeper;
using Lms.Rpc.Proxy;
using Lms.Transaction.Tcc;

namespace AnotherHostDemo
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(MessagePackModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule))]
    public class AnotherDemoModule : LmsModule
    {
       
    }
}