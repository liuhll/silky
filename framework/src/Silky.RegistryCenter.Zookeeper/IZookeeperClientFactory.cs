using System.Collections.Generic;
using Silky.Rpc.RegistryCenters;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    public interface IZookeeperClientFactory
    {
        IZookeeperClient GetZooKeeperClient();

        IReadOnlyList<IZookeeperClient> GetZooKeeperClients();

        RegistryCenterHealthCheckModel GetHealthCheckInfo(IZookeeperClient zookeeperClient);
    }
}