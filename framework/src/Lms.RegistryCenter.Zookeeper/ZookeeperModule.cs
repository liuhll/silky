using Lms.Core.Modularity;
using Lms.Rpc;

namespace Lms.RegistryCenter.Zookeeper
{
    [DependsOn(typeof(RpcModule))]
    public class ZookeeperModule : LmsModule
    {
    }
}