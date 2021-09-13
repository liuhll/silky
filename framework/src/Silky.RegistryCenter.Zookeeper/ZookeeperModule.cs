using System.Threading.Tasks;
using Silky.Core.Modularity;
using Silky.Rpc;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lock;
using Silky.RegistryCenter.Zookeeper.Routing;

namespace Silky.RegistryCenter.Zookeeper
{
    [DependsOn(typeof(RpcModule), typeof(LockModule))]
    public class ZookeeperModule : SilkyModule
    {
    }
}