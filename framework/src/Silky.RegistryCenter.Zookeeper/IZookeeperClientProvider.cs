using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Rpc.RegistryCenters;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    public interface IZookeeperClientProvider
    {
        IZookeeperClient GetZooKeeperClient();

        IReadOnlyList<IZookeeperClient> GetZooKeeperClients();

        RegistryCenterHealthCheckModel GetHealthCheckInfo(IZookeeperClient zookeeperClient);
    }
}