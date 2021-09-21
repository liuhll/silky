using System.Linq;
using System.Threading.Tasks;
using org.apache.zookeeper;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Routing;
using Silky.Rpc.Routing.Descriptor;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper.Routing.Watchers
{
    internal class ServiceRouteWatcher
    {
        internal string Path { get; }
        private readonly ServerRouteCache _serverRouteCache;
        private readonly ISerializer _serializer;
        private readonly object locker = new();
        public ServiceRouteWatcher(
            string path,
            ServerRouteCache serverRouteCache,
            ISerializer serializer)
        {
            Path = path;
            _serverRouteCache = serverRouteCache;
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
                    var serviceId = Path.Split("/").Last();
                    _serverRouteCache.RemoveCache(serviceId);
                    break;
                case Watcher.Event.EventType.NodeCreated:
                case Watcher.Event.EventType.NodeDataChanged:
                    lock (locker)
                    {
                        Check.NotNullOrEmpty(nodeData, nameof(nodeData));
                        var jonString = nodeData.GetString();
                        var serviceRouteDescriptor = _serializer.Deserialize<ServerRouteDescriptor>(jonString);
                        _serverRouteCache.UpdateCache(serviceRouteDescriptor);
                    }
                    break;
            }
        }

    }
}