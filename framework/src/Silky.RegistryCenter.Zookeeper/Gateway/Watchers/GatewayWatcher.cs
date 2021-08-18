using System.Linq;
using System.Threading.Tasks;
using org.apache.zookeeper;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Gateway;
using Silky.Rpc.Gateway.Descriptor;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper.Gateway.Watchers
{
    internal class GatewayWatcher
    {
        internal string Path { get; }
        private readonly GatewayCache _gatewayCache;
        private readonly ISerializer _serializer;

        public GatewayWatcher(string path, GatewayCache gatewayCache, ISerializer serializer)
        {
            Path = path;
            _gatewayCache = gatewayCache;
            _serializer = serializer;
        }

        public async Task HandleNodeDataChange(IZookeeperClient client, NodeDataChangeArgs args)
        {
            var eventType = args.Type;
            byte[] nodeData = null;
            if (args.CurrentData != null && args.CurrentData.Any())
            {
                nodeData = args.CurrentData.ToArray();
            }

            switch (eventType)
            {
                case Watcher.Event.EventType.NodeDeleted:
                    var hostName = Path.Split("/").Last();
                    _gatewayCache.RemoveCache(hostName);
                    break;
                case Watcher.Event.EventType.NodeCreated:
                    Check.NotNullOrEmpty(nodeData, nameof(nodeData));
                    var createdJsonString = nodeData.GetString();
                    var gatewayDescriptor = _serializer.Deserialize<GatewayDescriptor>(createdJsonString);
                    _gatewayCache.UpdateCache(gatewayDescriptor);
                    break;
                case Watcher.Event.EventType.NodeDataChanged:
                    Check.NotNullOrEmpty(nodeData, nameof(nodeData));
                    var updateJsonString = nodeData.GetString();
                    var updateGatewayDescriptor = _serializer.Deserialize<GatewayDescriptor>(updateJsonString);
                    _gatewayCache.UpdateCache(updateGatewayDescriptor);
                    break;
            }
        }
    }
}