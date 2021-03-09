using Autofac;
using Lms.Codec;
using Lms.Core.Modularity;
using Lms.DotNetty.Protocol.Tcp;
using Lms.RegistryCenter.Zookeeper;
using Lms.Transaction.Tcc;

namespace NormHostDemo
{
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule), 
        typeof(MessagePackModule),
        typeof(TransactionTccModule))]
    public class NormDemoModule : LmsModule
    {
       
    }
}