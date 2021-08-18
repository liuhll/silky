using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Lock.Extensions;
using Silky.RegistryCenter.Zookeeper.Gateway.Watchers;
using Silky.Rpc.Address;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Configuration;
using Silky.Rpc.Gateway;
using Silky.Rpc.Gateway.Descriptor;
using Silky.Rpc.Runtime.Server;
using Silky.Zookeeper;

namespace Silky.RegistryCenter.Zookeeper.Gateway
{
    public sealed class ZookeeperGatewayManager : IGatewayManager, ISingletonDependency
    {
        private readonly IZookeeperClientProvider _zookeeperClientProvider;
        private readonly ISerializer _serializer;
        private readonly RegistryCenterOptions _registryCenterOptions;
        private readonly GatewayCache _gatewayCache;
        private readonly IServiceEntryManager _serviceEntryManager;

        private ConcurrentDictionary<(string, IZookeeperClient), GatewayWatcher> m_gatewayWatchers = new();

        private ConcurrentDictionary<(string, IZookeeperClient), GatewaySubDirectoryWatcher>
            m_gatewaySubDirWatchers = new();

        public ILogger<ZookeeperGatewayManager> Logger { get; set; }

        public ZookeeperGatewayManager(IZookeeperClientProvider zookeeperClientProvider,
            ISerializer serializer,
            GatewayCache gatewayCache,
            IOptionsMonitor<RegistryCenterOptions> registryCenterOptions,
            IServiceEntryManager serviceEntryManager)
        {
            _zookeeperClientProvider = zookeeperClientProvider;
            _serializer = serializer;
            _gatewayCache = gatewayCache;
            _serviceEntryManager = serviceEntryManager;
            _registryCenterOptions = registryCenterOptions.CurrentValue;

            Logger = NullLogger<ZookeeperGatewayManager>.Instance;
        }

        public async Task EnterGateways()
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var gatewayDescriptors = await GetGatewayDescriptors(zookeeperClient);
                if (gatewayDescriptors.Any())
                {
                    foreach (var gatewayDescriptor in gatewayDescriptors)
                    {
                        _gatewayCache.UpdateCache(gatewayDescriptor);
                    }
                }
            }
        }

        public async Task RemoveGatewayAddress(string hostName, IAddressModel addressModel)
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var gatewayRoutePath = CreateGatewayRoutePath(hostName);
                var gatewayDescriptor = await GetGatewayDescriptorAsync(gatewayRoutePath, zookeeperClient);

                var synchronizationProvider = zookeeperClient.GetSynchronizationProvider();
                var @lock = synchronizationProvider.CreateLock(string.Format(LockName.RegisterGateway,
                    gatewayDescriptor.HostName));
                await @lock.ExecForHandle(async () =>
                {
                    gatewayDescriptor.Addresses = gatewayDescriptor.Addresses.Where(p => p != addressModel.Descriptor);
                    var jsonString = _serializer.Serialize(gatewayDescriptor);
                    var data = jsonString.GetBytes();
                    if (!await zookeeperClient.ExistsAsync(gatewayRoutePath))
                    {
                        await zookeeperClient.CreateRecursiveAsync(gatewayRoutePath, data, ZooDefs.Ids.OPEN_ACL_UNSAFE);
                        Logger.LogDebug($"Node {gatewayRoutePath} does not exist and will be created");
                    }
                    else
                    {
                        await zookeeperClient.SetDataAsync(gatewayRoutePath, data);
                        Logger.LogDebug($"The cached gateway data of the {gatewayRoutePath} node has been updated.");
                    }
                });
            }
        }

        public async Task RemoveLocalGatewayAddress()
        {
            var addressDescriptor = GetLocalAddressDescriptor();
            var gatewayDescriptor = CreateGatewayDescriptor(addressDescriptor);
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var synchronizationProvider = zookeeperClient.GetSynchronizationProvider();
                var @lock = synchronizationProvider.CreateLock(string.Format(LockName.RegisterGateway,
                    gatewayDescriptor.HostName));
                var gatewayRoutePath = CreateGatewayRoutePath(gatewayDescriptor.HostName);
                await @lock.ExecForHandle(async () =>
                {
                    gatewayDescriptor.Addresses = gatewayDescriptor.Addresses.Where(p => p != addressDescriptor);
                    var jsonString = _serializer.Serialize(gatewayDescriptor);
                    var data = jsonString.GetBytes();
                    if (!await zookeeperClient.ExistsAsync(gatewayRoutePath))
                    {
                        await zookeeperClient.CreateRecursiveAsync(gatewayRoutePath, data, ZooDefs.Ids.OPEN_ACL_UNSAFE);
                        Logger.LogDebug($"Node {gatewayRoutePath} does not exist and will be created");
                    }
                    else
                    {
                        await zookeeperClient.SetDataAsync(gatewayRoutePath, data);
                        Logger.LogDebug($"The cached gateway data of the {gatewayRoutePath} node has been updated.");
                    }
                });
            }
        }

        public async Task RegisterGateway()
        {
            await CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();

            var addressDescriptor = GetLocalAddressDescriptor();
            var gatewayDescriptor = CreateGatewayDescriptor(addressDescriptor);
            await RemoveExceptGatewayAsync(addressDescriptor);
            await RegisterGateway(gatewayDescriptor);
        }

        private async Task RemoveExceptGatewayAsync(AddressDescriptor addressDescriptor)
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var gatewayDescriptors = await GetGatewayDescriptors(zookeeperClient);
                var needRemoveAddressGatewayDescriptors = gatewayDescriptors.Where(p =>
                    p.HostName != EngineContext.Current.HostName &&
                    p.Addresses.Any(p => p == addressDescriptor));
                var synchronizationProvider = zookeeperClient.GetSynchronizationProvider();

                foreach (var needRemoveAddressGatewayDescriptor in needRemoveAddressGatewayDescriptors)
                {
                    var @lock = synchronizationProvider.CreateLock(string.Format(LockName.RegisterGateway,
                        needRemoveAddressGatewayDescriptor.HostName));
                    var gatewayRoutePath = CreateGatewayRoutePath(needRemoveAddressGatewayDescriptor.HostName);
                    await @lock.ExecForHandle(async () =>
                    {
                        needRemoveAddressGatewayDescriptor.Addresses =
                            needRemoveAddressGatewayDescriptor.Addresses.Where(p => p != addressDescriptor);
                        var jsonString = _serializer.Serialize(needRemoveAddressGatewayDescriptor);
                        var data = jsonString.GetBytes();
                        if (!await zookeeperClient.ExistsAsync(gatewayRoutePath))
                        {
                            await zookeeperClient.CreateRecursiveAsync(gatewayRoutePath, data,
                                ZooDefs.Ids.OPEN_ACL_UNSAFE);
                            Logger.LogDebug($"Node {gatewayRoutePath} does not exist and will be created");
                        }
                        else
                        {
                            await zookeeperClient.SetDataAsync(gatewayRoutePath, data);
                            Logger.LogDebug(
                                $"The cached gateway data of the {gatewayRoutePath} node has been updated.");
                        }
                    });
                }
            }
        }

        private AddressDescriptor GetLocalAddressDescriptor()
        {
            var server = EngineContext.Current.Resolve<IServer>();
            Check.NotNull(server, nameof(server));
            var address = server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
            if (address.IsNullOrEmpty())
            {
                throw new SilkyException("Failed to obtain http service address");
            }

            var addressInfo = ParseAddress(address);
            var addressDescriptor = new AddressDescriptor()
            {
                Address = addressInfo.Item1,
                Port = addressInfo.Item2,
                ServiceProtocol = ServiceProtocol.Http
            };
            return addressDescriptor;
        }

        private async Task RegisterGateway(GatewayDescriptor gatewayDescriptor)
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var synchronizationProvider = zookeeperClient.GetSynchronizationProvider();
                var @lock = synchronizationProvider.CreateLock(string.Format(LockName.RegisterGateway,
                    gatewayDescriptor.HostName));

                await @lock.ExecForHandle(async () =>
                {
                    var gatewayRoutePath = CreateGatewayRoutePath(gatewayDescriptor.HostName);
                    await CreateSubscribeDataChange(zookeeperClient, gatewayRoutePath);
                    var centreGatewaysDescriptor = await GetGatewayDescriptorAsync(gatewayRoutePath, zookeeperClient);
                    if (centreGatewaysDescriptor != null)
                    {
                        gatewayDescriptor.Addresses = gatewayDescriptor.Addresses
                            .Concat(centreGatewaysDescriptor.Addresses).Distinct().OrderBy(p => p.ToString());
                    }


                    var jsonString = _serializer.Serialize(gatewayDescriptor);
                    var data = jsonString.GetBytes();
                    if (!await zookeeperClient.ExistsAsync(gatewayRoutePath))
                    {
                        await zookeeperClient.CreateRecursiveAsync(gatewayRoutePath, data, ZooDefs.Ids.OPEN_ACL_UNSAFE);
                        Logger.LogDebug($"Node {gatewayRoutePath} does not exist and will be created");
                    }
                    else
                    {
                        await zookeeperClient.SetDataAsync(gatewayRoutePath, data);
                        Logger.LogDebug($"The cached gateway data of the {gatewayRoutePath} node has been updated.");
                    }
                });
            }
        }

        private async Task CreateSubDirectoryIfNotExistAndSubscribeChildrenChange()
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                await CreateSubDirectoryIfNotExistAndSubscribeChildrenChange(zookeeperClient);
            }
        }

        private async Task CreateSubDirectoryIfNotExistAndSubscribeChildrenChange(IZookeeperClient zookeeperClient)
        {
            var subDirectoryPath = _registryCenterOptions.GatewayPath;
            try
            {
                var synchronizationProvider = zookeeperClient.GetSynchronizationProvider();
                var @lock = synchronizationProvider.CreateLock(
                    string.Format(LockName.CreateGatewaySubDirectoryIfNotExistAndSubscribeChildrenChange,
                        subDirectoryPath.Replace("/", "_")));
                await @lock.ExecForHandle(async () =>
                {
                    if (!await zookeeperClient.ExistsAsync(subDirectoryPath))
                    {
                        await zookeeperClient.CreateRecursiveAsync(subDirectoryPath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE);
                    }
                });
            }
            catch (KeeperException.NodeExistsException e)
            {
                Logger.LogWarning("The directory {subDirectoryPath}has been created", e);
            }

            await CreateSubscribeChildrenChange(zookeeperClient, subDirectoryPath);
        }

        private async Task CreateSubscribeChildrenChange(IZookeeperClient zookeeperClient, string path)
        {
            if (!m_gatewaySubDirWatchers.ContainsKey((path, zookeeperClient)))
            {
                var watcher = new GatewaySubDirectoryWatcher(path, this);
                await zookeeperClient.SubscribeChildrenChange(path, watcher.SubscribeChildrenChange);
                m_gatewaySubDirWatchers.GetOrAdd((path, zookeeperClient), watcher);
            }
        }

        private GatewayDescriptor CreateGatewayDescriptor(AddressDescriptor addressDescriptor)
        {
            var allServiceEntries = _serviceEntryManager.GetAllEntries();
            var appServices = allServiceEntries.GroupBy(p => p.ServiceDescriptor.AppService).Select(p => p.Key);
            var gatewayServiceDescriptor = new GatewayDescriptor()
            {
                SupportServices = appServices,
                Addresses = new[] { addressDescriptor },
                HostName = EngineContext.Current.HostName
            };
            return gatewayServiceDescriptor;
        }

        private (string, int) ParseAddress(string address)
        {
            var domainAndPort = address.Split("//").Last().Split(":");
            var domain = domainAndPort[0];
            var port = int.Parse(domainAndPort[1]);
            return (domain, port);
        }

        public async Task CreateSubscribeGatewayDataChanges()
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                if (await zookeeperClient.ExistsAsync(_registryCenterOptions.GatewayPath))
                {
                    var children =
                        await zookeeperClient.GetChildrenAsync(_registryCenterOptions.GatewayPath);
                    foreach (var child in children)
                    {
                        var gatewayRoutePath = CreateGatewayRoutePath(child);
                        await CreateSubscribeDataChange(zookeeperClient, gatewayRoutePath);
                    }
                }
            }
        }

        private async Task<IReadOnlyCollection<GatewayDescriptor>> GetGatewayDescriptors(
            IZookeeperClient zookeeperClient)
        {
            var gatewayDescriptors = new List<GatewayDescriptor>();
            if (await zookeeperClient.ExistsAsync(_registryCenterOptions.GatewayPath))
            {
                var children =
                    await zookeeperClient.GetChildrenAsync(_registryCenterOptions.GatewayPath);
                foreach (var child in children)
                {
                    var gatewayRoutePath = CreateGatewayRoutePath(child);
                    var serviceRouteDescriptor = await GetGatewayDescriptorAsync(gatewayRoutePath, zookeeperClient);
                    gatewayDescriptors.Add(serviceRouteDescriptor);
                }
            }

            return gatewayDescriptors;
        }

        private string CreateGatewayRoutePath(string hostName)
        {
            Check.NotNullOrEmpty(_registryCenterOptions.GatewayPath, nameof(_registryCenterOptions.GatewayPath));
            Check.NotNullOrEmpty(hostName, nameof(hostName));
            var gatewayPath = _registryCenterOptions.GatewayPath.EndsWith("/")
                ? _registryCenterOptions.GatewayPath
                : _registryCenterOptions.GatewayPath + "/";
            var gatewayRoutePath = gatewayPath += hostName;
            return gatewayRoutePath;
        }

        private async Task<GatewayDescriptor> GetGatewayDescriptorAsync(string gatewayPath,
            IZookeeperClient zookeeperClient)
        {
            if (!await zookeeperClient.ExistsAsync(gatewayPath))
            {
                return null;
            }

            var data = await zookeeperClient.GetDataAsync(gatewayPath);
            if (data == null || !data.Any())
            {
                return null;
            }

            var jsonString = data.ToArray().GetString();
            return _serializer.Deserialize<GatewayDescriptor>(jsonString);
        }

        internal async Task CreateSubscribeDataChange(IZookeeperClient zookeeperClient, string path)
        {
            if (!m_gatewayWatchers.ContainsKey((path, zookeeperClient)))
            {
                var watcher = new GatewayWatcher(path, _gatewayCache, _serializer);
                await zookeeperClient.SubscribeDataChange(path, watcher.HandleNodeDataChange);
                m_gatewayWatchers.GetOrAdd((path, zookeeperClient), watcher);
            }
        }
    }
}