using System.Collections.Generic;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Zookeeper;

namespace Silky.Lms.RegistryCenter.Zookeeper
{
    public interface IZookeeperClientProvider : ISingletonDependency
    {
        IZookeeperClient GetZooKeeperClient();

        IReadOnlyList<IZookeeperClient> GetZooKeeperClients();
    }
}