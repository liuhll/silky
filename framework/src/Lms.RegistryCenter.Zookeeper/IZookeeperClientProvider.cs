using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Zookeeper;

namespace Lms.RegistryCenter.Zookeeper
{
    internal interface IZookeeperClientProvider : ISingletonDependency
    {
        Task<IZookeeperClient> GetZooKeeperClient();

        Task<IEnumerable<IZookeeperClient>> GetZooKeeperClients();
    }
}