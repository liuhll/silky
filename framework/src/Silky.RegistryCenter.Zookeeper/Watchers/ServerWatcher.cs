using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using org.apache.zookeeper;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper.Watchers
{
    public class ServerWatcher
    {
        protected string SubDirectoryPath { get; }
        private readonly ZookeeperServerRegister _zookeeperServerRegister;
        private readonly ISerializer _serializer;

        public ServerWatcher(string subDirectoryPath,
            ZookeeperServerRegister zookeeperServerRegister,
            ISerializer serializer)
        {
            SubDirectoryPath = subDirectoryPath;
            _zookeeperServerRegister = zookeeperServerRegister;
            _serializer = serializer;
        }


        public async Task SubscribeServerChange(IZookeeperClient client, NodeDataChangeArgs args)
        {
            var eventType = args.Type;
            byte[] nodeData = null;
            if (args.CurrentData != null && args.CurrentData.Any())
            {
                nodeData = args.CurrentData.ToArray();
            }

            switch (eventType)
            {
                case Watcher.Event.EventType.NodeCreated:
                case Watcher.Event.EventType.NodeDeleted:
                    break;
                case Watcher.Event.EventType.NodeDataChanged:
                    Check.NotNull(nodeData, nameof(nodeData));
                    if (!nodeData.Any())
                    {
                        return;
                    }

                    var jonString = nodeData.GetString();
                    var allServers = _serializer.Deserialize<List<string>>(jonString);
                    foreach (var server in allServers)
                    {
                        await _zookeeperServerRegister.CreateSubscribeDataChange(client, server);
                        await _zookeeperServerRegister.UpdateServerRouteCache(client, server);
                    }

                    break;
            }
        }
    }
}