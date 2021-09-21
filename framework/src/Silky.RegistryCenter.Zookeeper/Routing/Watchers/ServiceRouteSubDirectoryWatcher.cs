using System.Threading.Tasks;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper.Routing.Watchers
{
    public class ServiceRouteSubDirectoryWatcher
    {
        protected string SubDirectoryPath { get; }
        private readonly ZookeeperServerRouteManager _zookeeperServerRouteManager;

        public ServiceRouteSubDirectoryWatcher(string subDirectoryPath, ZookeeperServerRouteManager zookeeperServerRouteManager)
        {
            SubDirectoryPath = subDirectoryPath;
            _zookeeperServerRouteManager = zookeeperServerRouteManager;
        }


        public async Task SubscribeChildrenChange(IZookeeperClient client, NodeChildrenChangeArgs args)
        {
            var currentChildrens = args.CurrentChildrens;
            foreach (var child in currentChildrens)
            {
                await _zookeeperServerRouteManager.CreateSubscribeDataChange(client, child);
            }
        }
    }
}