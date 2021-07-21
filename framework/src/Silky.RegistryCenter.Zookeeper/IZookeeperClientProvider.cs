using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    public interface IZookeeperClientProvider : ISingletonDependency
    {
        IZookeeperClient GetZooKeeperClient();

        IReadOnlyList<IZookeeperClient> GetZooKeeperClients();
    }
}