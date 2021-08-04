using Medallion.Threading.ZooKeeper;
using Silky.Core;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    public static class ZookeeperClientExtensions
    {
        public static ZooKeeperDistributedSynchronizationProvider GetSynchronizationProvider(
            this IZookeeperClient zookeeperClient)
        {
            return Singleton<ZooKeeperDistributedSynchronizationProvider>.Instance ??
                   (Singleton<ZooKeeperDistributedSynchronizationProvider>.Instance =
                       new ZooKeeperDistributedSynchronizationProvider(zookeeperClient.Options.ConnectionString));
        }
    }
}