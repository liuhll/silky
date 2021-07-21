using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Routing;
using Silky.Rpc.Routing.Descriptor;
using Silky.Zookeeper;
using org.apache.zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    internal class ServiceRouteWatcher
    {
        internal string Path { get; }
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly ISerializer _serializer;

        public ServiceRouteWatcher(
            string path,
            ServiceRouteCache serviceRouteCache,
            ISerializer serializer)
        {
            Path = path;
            _serviceRouteCache = serviceRouteCache;
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
                    _serviceRouteCache.RemoveCache(serviceId);
                    break;
                case Watcher.Event.EventType.NodeCreated:
                    Check.NotNullOrEmpty(nodeData, nameof(nodeData));
                    var createdJsonString = nodeData.GetString();
                    var createdServiceRouteDescriptor = _serializer.Deserialize<ServiceRouteDescriptor>(createdJsonString);
                    _serviceRouteCache.UpdateCache(createdServiceRouteDescriptor);
                    break;
                case Watcher.Event.EventType.NodeDataChanged:
                    Check.NotNullOrEmpty(nodeData, nameof(nodeData));
                    var updateJsonString = nodeData.GetString();
                    var updateServiceRouteDescriptor = _serializer.Deserialize<ServiceRouteDescriptor>(updateJsonString);
                    _serviceRouteCache.UpdateCache(updateServiceRouteDescriptor);
                    break;
            }
        }

    }
}