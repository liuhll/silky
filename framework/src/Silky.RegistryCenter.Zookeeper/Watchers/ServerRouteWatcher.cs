using System.Linq;
using System.Threading.Tasks;
using org.apache.zookeeper;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper.Watchers
{
    internal class ServerRouteWatcher
    {
        internal string Path { get; }
        private readonly IServerManager _serverManager;
        private readonly ISerializer _serializer;
        private readonly object locker = new();

        public ServerRouteWatcher(
            string path,
            IServerManager serverManager,
            ISerializer serializer)
        {
            Path = path;
            _serverManager = serverManager;
            _serializer = serializer;
        }

        internal async Task HandleNodeDataChange(IZookeeperClient client, NodeDataChangeArgs args)
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
                    _serverManager.Remove(hostName);
                    break;
                case Watcher.Event.EventType.NodeCreated:
                case Watcher.Event.EventType.NodeDataChanged:
                    lock (locker)
                    {
                        Check.NotNullOrEmpty(nodeData, nameof(nodeData));
                        var jonString = nodeData.GetString();
                        var serverRouteDescriptor = _serializer.Deserialize<ServerDescriptor>(jonString);
                        _serverManager.Update(serverRouteDescriptor);
                    }

                    break;
            }
        }
    }
}