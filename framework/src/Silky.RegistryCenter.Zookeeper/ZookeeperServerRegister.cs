using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Zookeeper.Configuration;
using Silky.RegistryCenter.Zookeeper.Watchers;
using Silky.Rpc.Endpoint;
using Silky.Rpc.RegistryCenters.HeartBeat;
using Silky.Rpc.Runtime.Server;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper
{
    public class ZookeeperServerRegister : ServerRegisterBase, IZookeeperStatusChange
    {
        private readonly IZookeeperClientFactory _zookeeperClientFactory;
        private readonly ISerializer _serializer;
        public ILogger<ZookeeperServerRegister> Logger { get; set; }

        private ConcurrentDictionary<(string, IZookeeperClient), ServerRouteWatcher> m_serviceRouteWatchers = new();
        private ConcurrentDictionary<IZookeeperClient, ServerWatcher> m_serverWatchers = new();
        private ZookeeperRegistryCenterOptions _registryCenterOptions;
        private readonly IHeartBeatService _heartBeatService;

        public ZookeeperServerRegister(IServerManager serverManager,
            IServerProvider serverProvider,
            IZookeeperClientFactory zookeeperClientFactory,
            IOptionsMonitor<ZookeeperRegistryCenterOptions> registryCenterOptions,
            ISerializer serializer,
            IHeartBeatService heartBeatService)
            : base(serverManager,
                serverProvider)
        {
            _zookeeperClientFactory = zookeeperClientFactory;
            _serializer = serializer;
            _heartBeatService = heartBeatService;
            _registryCenterOptions = registryCenterOptions.CurrentValue;
            Check.NotNullOrEmpty(_registryCenterOptions.RoutePath, nameof(_registryCenterOptions.RoutePath));
            Logger = NullLogger<ZookeeperServerRegister>.Instance;
        }

        public override async Task RegisterServer()
        {
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                await CreateSubscribeServersChange(zookeeperClient);
            }

            await base.RegisterServer();
        }

        protected override async Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor)
        {
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var routePath = CreateRoutePath(serverDescriptor.HostName);
                // The latest routing data must be obtained from the service registry.
                // When the service is expanded and contracted, the locally cached routing data is not the latest
                var centreServiceRoute = await GetRouteDescriptorAsync(zookeeperClient, routePath);
                if (centreServiceRoute != null)
                {
                    serverDescriptor.Endpoints = serverDescriptor.Endpoints
                        .Concat(centreServiceRoute.Endpoints)
                        .Distinct()
                        .OrderBy(p => p.ToString()).ToArray();
                }

                var jsonString = _serializer.Serialize(serverDescriptor);
                var data = jsonString.GetBytes();
                if (!await zookeeperClient.ExistsAsync(routePath))
                {
                    await zookeeperClient.CreateRecursiveAsync(routePath, data, ZooDefs.Ids.OPEN_ACL_UNSAFE);
                    await RegisterServer(zookeeperClient);
                    Logger.LogDebug($"Node {routePath} does not exist and will be created");
                }
                else
                {
                    await zookeeperClient.SetDataAsync(routePath, data);
                    Logger.LogDebug($"The cached server data of the {routePath} node has been updated");
                }
            }
        }

        private async Task RegisterServer(IZookeeperClient zookeeperClient)
        {
            var routePath = _registryCenterOptions.RoutePath;
            var allServers = await GetAllServers(zookeeperClient, routePath);
            allServers.Add(EngineContext.Current.HostName);
            var jonString = _serializer.Serialize(allServers);
            var data = jonString.GetBytes();
            await zookeeperClient.SetDataAsync(routePath, data);
            var serverRoutePath = CreateRoutePath(EngineContext.Current.HostName);
            if (!m_serviceRouteWatchers.ContainsKey((serverRoutePath, zookeeperClient)))
            {
                var serverRouteWatcher = new ServerRouteWatcher(serverRoutePath, _serverManager, _serializer);
                await zookeeperClient.SubscribeDataChange(serverRoutePath, serverRouteWatcher.HandleNodeDataChange);
                m_serviceRouteWatchers.GetOrAdd((serverRoutePath, zookeeperClient), serverRouteWatcher);
            }
        }

        protected override async Task RemoveServiceCenterExceptRpcEndpoint(IServer server)
        {
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var allServerRouteDescriptors = await GetServerRouteDescriptors(zookeeperClient);
                var serviceRouteDescriptors = allServerRouteDescriptors as ServerDescriptor[] ??
                                              allServerRouteDescriptors.ToArray();
                if (!serviceRouteDescriptors.Any())
                {
                    continue;
                }

                foreach (var localEndpoint in server.Endpoints)
                {
                    var removeExceptRouteDescriptors = serviceRouteDescriptors.Where(p =>
                        p.Endpoints.Any(e => e.Equals(localEndpoint.Descriptor))
                        && !p.HostName.Equals(EngineContext.Current.HostName)
                    );
                    if (removeExceptRouteDescriptors.Any())
                    {
                        foreach (var removeExceptRouteDescriptor in removeExceptRouteDescriptors)
                        {
                            var routePath = CreateRoutePath(removeExceptRouteDescriptor.HostName);
                            removeExceptRouteDescriptor.Endpoints = removeExceptRouteDescriptor.Endpoints
                                .Where(p => !p.Equals(localEndpoint.Descriptor))
                                .ToArray();
                            var jsonString = _serializer.Serialize(removeExceptRouteDescriptor);
                            var data = jsonString.GetBytes();
                            await zookeeperClient.SetDataAsync(routePath, data);
                        }
                    }
                }
            }
        }

        protected override async Task CacheServers()
        {
            await CacheServersFromZookeeper();
            if (_registryCenterOptions.EnableHeartBeat)
            {
                _heartBeatService.Start(CacheServersFromZookeeper);
            }
        }

        private async Task CacheServersFromZookeeper()
        {
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var serviceRouteDescriptors =
                    await GetServerRouteDescriptors(zookeeperClient);
                var serverDescriptors = serviceRouteDescriptors as ServerDescriptor[] ??
                                        serviceRouteDescriptors.ToArray();
                if (serverDescriptors.Any())
                {
                    foreach (var serviceRouteDescriptor in serverDescriptors)
                    {
                        _serverManager.Update(serviceRouteDescriptor);
                    }

                    break;
                }
            }
        }

        protected override async Task RemoveRpcEndpoint(string hostName, IRpcEndpoint rpcEndpoint)
        {
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var routePath = CreateRoutePath(hostName);

                var serviceCenterDescriptor = await GetRouteDescriptorAsync(zookeeperClient, routePath);
                if (serviceCenterDescriptor != null &&
                    serviceCenterDescriptor.Endpoints.Any(p => p.Equals(rpcEndpoint.Descriptor)))
                {
                    serviceCenterDescriptor.Endpoints = serviceCenterDescriptor.Endpoints
                        .Where(p => !p.Equals(rpcEndpoint.Descriptor))
                        .ToArray();
                    var jsonString = _serializer.Serialize(serviceCenterDescriptor);
                    var data = jsonString.GetBytes();
                    await zookeeperClient.SetDataAsync(routePath, data);
                }
            }
        }

        private async Task<IEnumerable<ServerDescriptor>> GetServerRouteDescriptors(IZookeeperClient zookeeperClient)
        {
            var serverRouteDescriptors = new List<ServerDescriptor>();
            var children = await zookeeperClient.GetChildrenAsync(_registryCenterOptions.RoutePath);
            foreach (var child in children)
            {
                var routePath = CreateRoutePath(child);
                if (await zookeeperClient.ExistsAsync(routePath))
                {
                    var serverRouteDescriptor = await GetRouteDescriptorAsync(zookeeperClient, routePath);
                    serverRouteDescriptors.Add(serverRouteDescriptor);
                }
            }

            return serverRouteDescriptors;
        }

        public async Task CreateSubscribeServersChange(IZookeeperClient zookeeperClient)
        {
            var serverPath = _registryCenterOptions.RoutePath;

            var watcher = new ServerWatcher(serverPath, this, _serializer);
            await zookeeperClient.SubscribeDataChange(serverPath, watcher.SubscribeServerChange);
            m_serverWatchers.AddOrUpdate(zookeeperClient, watcher, (k, v) => watcher);
            if (!await zookeeperClient.ExistsAsync(serverPath))
            {
                await zookeeperClient.CreateRecursiveAsync(serverPath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE);
                return;
            }

            var allServers = await GetAllServers(zookeeperClient, serverPath);
            foreach (var server in allServers)
            {
                await CreateSubscribeDataChange(zookeeperClient, server);
            }
        }

        private async Task<List<string>> GetAllServers(IZookeeperClient zookeeperClient, string serverPath)
        {
            var allServers = new List<string>();
            var datas = await zookeeperClient.GetDataAsync(serverPath);
            if (datas == null)
            {
                return allServers;
            }

            var jsonString = datas.ToArray().GetString();
            allServers = _serializer.Deserialize<List<string>>(jsonString);
            return allServers;
        }

        internal async Task UpdateServerRouteCache(IZookeeperClient zookeeperClient, string path)
        {
            var routePath = CreateRoutePath(path);
            var centerServiceServerRoute = await GetRouteDescriptorAsync(zookeeperClient, routePath);
            if (centerServiceServerRoute != null)
            {
                _serverManager.Update(centerServiceServerRoute);
            }
        }

        internal async Task CreateSubscribeDataChange(IZookeeperClient zookeeperClient, string path)
        {
            var routePath = CreateRoutePath(path);
            var watcher = new ServerRouteWatcher(routePath, _serverManager, _serializer);
            await zookeeperClient.SubscribeDataChange(routePath, watcher.HandleNodeDataChange);
            m_serviceRouteWatchers.GetOrAdd((routePath, zookeeperClient), watcher);
        }

        private async Task<ServerDescriptor> GetRouteDescriptorAsync(IZookeeperClient zookeeperClient,
            string routePath)
        {
            if (!await zookeeperClient.ExistsAsync(routePath))
            {
                return null;
            }

            var data = await zookeeperClient.GetDataAsync(routePath);
            if (data == null || !data.Any())
            {
                return null;
            }

            var jsonString = data.ToArray().GetString();
            return _serializer.Deserialize<ServerDescriptor>(jsonString);
        }

        private string CreateRoutePath(string child)
        {
            var routePath = _registryCenterOptions.RoutePath;
            if (!routePath.EndsWith("/"))
            {
                routePath += "/";
            }

            routePath += child;
            return routePath;
        }
    }
}