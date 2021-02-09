using System.Linq;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Extensions;
using Lms.Core.Serialization;
using Lms.Rpc.Routing;
using Lms.Rpc.Routing.Descriptor;
using Lms.Zookeeper;
using org.apache.zookeeper;

namespace Lms.RegistryCenter.Zookeeper
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