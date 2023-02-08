using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.DistributedLock.Redis;
using Silky.RegistryCenter.Zookeeper.Configuration;
using Silky.RegistryCenter.Zookeeper.Watchers;
using Silky.Rpc.Endpoint;
using Silky.Rpc.RegistryCenters.HeartBeat;
using Silky.Rpc.Runtime.Server;
using Silky.Zookeeper;
using StackExchange.Redis;
using IServer = Silky.Rpc.Runtime.Server.IServer;

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
        private readonly IRedisDistributedLockProvider _distributedLockProvider;

        public ZookeeperServerRegister(IServerManager serverManager,
            IServerProvider serverProvider,
            IZookeeperClientFactory zookeeperClientFactory,
            IOptionsMonitor<ZookeeperRegistryCenterOptions> registryCenterOptions,
            ISerializer serializer,
            IHeartBeatService heartBeatService,
            IRedisDistributedLockProvider distributedLockProvider)
            : base(serverManager,
                serverProvider)
        {
            _zookeeperClientFactory = zookeeperClientFactory;
            _serializer = serializer;
            _heartBeatService = heartBeatService;
            _distributedLockProvider = distributedLockProvider;
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
            var routePath = CreateRoutePath(serverDescriptor.HostName);
            var redisOptions = EngineContext.Current.Configuration.GetRedisCacheOptions();
            foreach (var zookeeperClient in zookeeperClients)
            {
                using var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions.Configuration);
                var @lock = _distributedLockProvider.Create(connection.GetDatabase(),  $"RegisterServerToServiceCenter:Zookeeper:{zookeeperClient.Options.ConnectionString.Replace(":", "")}");
                await using (await @lock.AcquireAsync())
                {
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
                        await RegisterServerName(zookeeperClient);
                        await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
                        await zookeeperClient.CreateRecursiveAsync(routePath, data,
                            AclUtils.GetAcls(_registryCenterOptions.Scheme, _registryCenterOptions.Auth));
                        Logger.LogDebug($"Node {routePath} does not exist and will be created");
                    }
                    else
                    {
                        await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
                        await zookeeperClient.SetDataAsync(routePath, data);
                        Logger.LogDebug($"The cached server data of the {routePath} node has been updated");
                    }
                    await CreateServiceRouteWatcher(zookeeperClient, routePath);
                }
            }
        }

        private async Task RegisterServerName(IZookeeperClient zookeeperClient)
        {
            var routePath = _registryCenterOptions.RoutePath;
            var allServers = await GetAllServers(zookeeperClient, routePath);
            if (allServers.Contains(EngineContext.Current.HostName))
            {
                return;
            }

            allServers.Add(EngineContext.Current.HostName);
            var jonString = _serializer.Serialize(allServers);
            var data = jonString.GetBytes();
            await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
            await zookeeperClient.SetDataAsync(routePath, data);
        }

        protected override async Task RemoveServiceCenterExceptRpcEndpoint(IServer server)
        {
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var allServerRouteDescriptors = await GetServerRouteDescriptors(zookeeperClient);

                if (!allServerRouteDescriptors.Any())
                {
                    continue;
                }

                foreach (var localEndpoint in server.Endpoints)
                {
                    var removeExceptRouteDescriptors = allServerRouteDescriptors.Where(p =>
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
                            await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
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
                _heartBeatService.Start(HeartBeatServers);
            }
        }

        private async Task HeartBeatServers()
        {
            if (!await RepeatRegister())
            {
                await CacheServersFromZookeeper();
            }
        }

        private async Task CacheServersFromZookeeper()
        {
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var serviceRouteDescriptors =
                    await GetServerRouteDescriptors(zookeeperClient);

                if (serviceRouteDescriptors.Any())
                {
                    _serverManager.UpdateAll(serviceRouteDescriptors);
                    break;
                }
            }
        }

        protected override async Task RemoveSilkyEndpoint(string hostName, ISilkyEndpoint silkyEndpoint)
        {
            var zookeeperClients = _zookeeperClientFactory.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var routePath = CreateRoutePath(hostName);

                var serviceCenterDescriptor = await GetRouteDescriptorAsync(zookeeperClient, routePath);
                if (serviceCenterDescriptor != null &&
                    serviceCenterDescriptor.Endpoints.Any(p => p.Equals(silkyEndpoint.Descriptor)))
                {
                    serviceCenterDescriptor.Endpoints = serviceCenterDescriptor.Endpoints
                        .Where(p => !p.Equals(silkyEndpoint.Descriptor))
                        .ToArray();
                    var jsonString = _serializer.Serialize(serviceCenterDescriptor);
                    var data = jsonString.GetBytes();
                    await zookeeperClient.SetDataAsync(routePath, data);
                }
            }
        }

        private async Task<ServerDescriptor[]> GetServerRouteDescriptors(IZookeeperClient zookeeperClient)
        {
            var serverRouteDescriptors = new List<ServerDescriptor>();
            await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
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

            return serverRouteDescriptors.ToArray();
        }

        public async Task CreateSubscribeServersChange(IZookeeperClient zookeeperClient)
        {
            var serverPath = _registryCenterOptions.RoutePath;

            var redisOptions = EngineContext.Current.Configuration.GetRedisCacheOptions();
            
            using var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions.Configuration);
            var @lock = _distributedLockProvider.Create(connection.GetDatabase(),$"CreateSubscribeServersChange:Zookeeper:{zookeeperClient.Options.ConnectionString.Replace(":", "")}");
            await using (await @lock.AcquireAsync())
            {
                if (!await zookeeperClient.ExistsAsync(serverPath))
                {
                    await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
                    await zookeeperClient.CreateRecursiveAsync(serverPath, null,
                        AclUtils.GetAcls(_registryCenterOptions.Scheme, _registryCenterOptions.Auth));
                }

                var watcher = new ServerWatcher(serverPath, this, _serializer);
                await zookeeperClient.SubscribeDataChange(serverPath, watcher.SubscribeServerChange);
                m_serverWatchers.AddOrUpdate(zookeeperClient, watcher, (k, v) => watcher);
            }

            var allServers = await GetAllServers(zookeeperClient, serverPath);
            foreach (var server in allServers)
            {
                var serverRoutePath = CreateRoutePath(server);
                await CreateServiceRouteWatcher(zookeeperClient, serverRoutePath);
            }
        }

        private async Task CreateServiceRouteWatcher(IZookeeperClient zookeeperClient, string serverRoutePath)
        {
            if (!m_serviceRouteWatchers.ContainsKey((serverRoutePath, zookeeperClient)))
            {
                var serverRouteWatcher = new ServerRouteWatcher(serverRoutePath, _serverManager, _serializer);
                await zookeeperClient.SubscribeDataChange(serverRoutePath,
                    serverRouteWatcher.HandleNodeDataChange);
                m_serviceRouteWatchers.GetOrAdd((serverRoutePath, zookeeperClient), serverRouteWatcher);
            }
        }

        private async Task<List<string>> GetAllServers(IZookeeperClient zookeeperClient, string serverPath)
        {
            var allServers = new List<string>();
            await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
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

            await zookeeperClient.Authorize(_registryCenterOptions.Scheme, _registryCenterOptions.Auth);
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