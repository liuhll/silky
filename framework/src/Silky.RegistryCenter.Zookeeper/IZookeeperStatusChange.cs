using System.Threading.Tasks;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    internal interface IZookeeperStatusChange
    {
        Task CreateSubscribeServersChange(IZookeeperClient zookeeperClient);
    }
}