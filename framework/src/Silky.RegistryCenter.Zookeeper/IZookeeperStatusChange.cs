using System.Threading.Tasks;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    public interface IZookeeperStatusChange
    {
        Task CreateSubscribeServersChange(IZookeeperClient zookeeperClient);
    }
}