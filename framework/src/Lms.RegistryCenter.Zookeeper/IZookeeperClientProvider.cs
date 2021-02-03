using System.Collections.Generic;
using Lms.Core.DependencyInjection;
using Lms.Zookeeper;

namespace Lms.RegistryCenter.Zookeeper
{
    public interface IZookeeperClientProvider : ISingletonDependency
    {
        IZookeeperClient GetZooKeeperClient();

        IReadOnlyList<IZookeeperClient> GetZooKeeperClients();
    }
}