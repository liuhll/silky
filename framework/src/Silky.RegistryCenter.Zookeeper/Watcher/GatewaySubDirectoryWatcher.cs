using System.Threading.Tasks;
using Silky.RegistryCenter.Zookeeper.Gateway;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    internal class GatewaySubDirectoryWatcher
    {
        protected string SubDirectoryPath { get; }
        private readonly ZookeeperGatewayManager _zookeeperGatewayManager;

        public GatewaySubDirectoryWatcher(string subDirectoryPath, ZookeeperGatewayManager zookeeperGatewayManager)
        {
            SubDirectoryPath = subDirectoryPath;
            _zookeeperGatewayManager = zookeeperGatewayManager;
        }


        public async Task SubscribeChildrenChange(IZookeeperClient client, NodeChildrenChangeArgs args)
        {
            var currentChildrens = args.CurrentChildrens;
            foreach (var child in currentChildrens)
            {
                await _zookeeperGatewayManager.CreateSubscribeDataChange(client, child);
            }
        }
    }
}