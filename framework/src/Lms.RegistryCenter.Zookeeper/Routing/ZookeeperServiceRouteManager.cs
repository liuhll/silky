using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Core.Extensions;
using Lms.Core.Serialization;
using Lms.Lock.Provider;
using Lms.Rpc.Address.Descriptor;
using Lms.Rpc.Configuration;
using Lms.Rpc.Routing;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.Descriptor;
using Lms.Rpc.Utils;
using Lms.Zookeeper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;

namespace Lms.RegistryCenter.Zookeeper.Routing
{
    public class ZookeeperServiceRouteManager : ServiceRouteManagerBase, ISingletonDependency
    {
        private readonly IZookeeperClientProvider _zookeeperClientProvider;
        private readonly ISerializer _serializer;
        public ILogger<ZookeeperServiceRouteManager> Logger { get; set; }

        private ConcurrentDictionary<(string, IZookeeperClient), ServiceRouteWatcher> m_routeWatchers = new();

        private ConcurrentDictionary<(string, IZookeeperClient), ServiceRouteSubDirectoryWatcher>
            m_routeSubDirWatchers = new();


        public ZookeeperServiceRouteManager(ServiceRouteCache serviceRouteCache,
            IServiceEntryManager serviceEntryManager,
            IZookeeperClientProvider zookeeperClientProvider,
            ILockerProvider lockerProvider,
            IOptions<RegistryCenterOptions> registryCenterOptions,
            IOptions<RpcOptions> rpcOptions,
            ISerializer serializer)
            : base(serviceRouteCache,
                serviceEntryManager,
                lockerProvider,
                registryCenterOptions,
                rpcOptions)
        {
            _zookeeperClientProvider = zookeeperClientProvider;
            _serializer = serializer;
            Logger = NullLogger<ZookeeperServiceRouteManager>.Instance;
        }


        protected async override Task RegisterRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor)
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var routePath = CreateRoutePath(serviceRouteDescriptor.ServiceDescriptor);
                var jsonString = _serializer.Serialize(serviceRouteDescriptor);
                var data = jsonString.GetBytes();
                if (!await zookeeperClient.ExistsAsync(routePath))
                {
                    Logger.LogDebug($"节点{routePath}不存在将进行创建");
                    await zookeeperClient.CreateRecursiveAsync(routePath, data, ZooDefs.Ids.OPEN_ACL_UNSAFE);
                }
                else
                {
                    var onlineData = (await zookeeperClient.GetDataAsync(routePath)).ToArray();
                    if (!onlineData.Equals(data))
                    {
                        await zookeeperClient.SetDataAsync(routePath, data);
                        Logger.LogDebug($"{routePath}节点的缓存的路由数据已被更新。");
                    }
                }
            }
        }

        protected async override Task CreateSubDirectory()
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                await CreateSubDirectory(zookeeperClient);
            }
        }

        private async Task CreateSubDirectory(IZookeeperClient zookeeperClient)
        {
            var subDirectoryPath = _registryCenterOptions.RoutePath;

            if (!await zookeeperClient.ExistsAsync(subDirectoryPath))
            {
                await zookeeperClient.CreateRecursiveAsync(subDirectoryPath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE);
            }

            await CreateSubscribeChildrenChange(zookeeperClient, subDirectoryPath);
        }

        public override async Task EnterRoutes()
        {
            var zookeeperClient = _zookeeperClientProvider.GetZooKeeperClient();

            if (await zookeeperClient.ExistsAsync(_registryCenterOptions.RoutePath))
            {
                var children =
                    await zookeeperClient.GetChildrenAsync(_registryCenterOptions.RoutePath);
                foreach (var child in children)
                {
                    var routePath = CreateRoutePath(child);
                    var serviceRouteDescriptor = await GetRouteDescriptorAsync(routePath, zookeeperClient);
                    if (serviceRouteDescriptor != null)
                    {
                        _serviceRouteCache.UpdateCache(serviceRouteDescriptor);
                    }
                }
            }
        }


        private async Task<ServiceRouteDescriptor> GetRouteDescriptorAsync(string routePath,
            IZookeeperClient zookeeperClient)
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
            return _serializer.Deserialize<ServiceRouteDescriptor>(jsonString);
        }

        private string CreateRoutePath(ServiceDescriptor serviceDescriptor)
        {
            return CreateRoutePath(serviceDescriptor.Id);
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

        public async override Task CreateSubscribeDataChanges()
        {
            var allServiceEntries = _serviceEntryManager.GetAllEntries();
            foreach (var serviceEntry in allServiceEntries)
            {
                var serviceRoutePath = CreateRoutePath(serviceEntry.ServiceDescriptor.Id);
                var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
                foreach (var zookeeperClient in zookeeperClients)
                {
                    await CreateSubscribeDataChange(zookeeperClient, serviceRoutePath);
                }
            }
        }

        public async override Task CreateWsSubscribeDataChanges(string[] wsPaths)
        {
            foreach (var wsPath in wsPaths)
            {
                var wsServiceId = WebSocketResolverHelper.Generator(wsPath);
                var serviceRoutePath = CreateRoutePath(wsServiceId);
                var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
                foreach (var zookeeperClient in zookeeperClients)
                {
                    await CreateSubscribeDataChange(zookeeperClient, serviceRoutePath);
                }
            }
        }

        internal async Task CreateSubscribeDataChange(IZookeeperClient zookeeperClient, string path)
        {
            if (!m_routeWatchers.ContainsKey((path, zookeeperClient)))
            {
                var watcher = new ServiceRouteWatcher(path, _serviceRouteCache, _serializer);
                await zookeeperClient.SubscribeDataChange(path, watcher.HandleNodeDataChange);
                m_routeWatchers.GetOrAdd((path, zookeeperClient), watcher);
            }
        }

        private async Task CreateSubscribeChildrenChange(IZookeeperClient zookeeperClient, string path)
        {
            if (!m_routeSubDirWatchers.ContainsKey((path, zookeeperClient)))
            {
                var watcher = new ServiceRouteSubDirectoryWatcher(path, this);
                await zookeeperClient.SubscribeChildrenChange(path, watcher.SubscribeChildrenChange);
                m_routeSubDirWatchers.GetOrAdd((path, zookeeperClient), watcher);
            }
        }

        public async Task RemoveLocalHostServiceRoute()
        {
            var serviceRouteDescriptors = _serviceRouteCache.ServiceRouteDescriptors
                .Where(p => p.AddressDescriptors.Any(p =>
                    p.Address == NetUtil.GetHostAddress(_rpcOptions.Host)));

            foreach (var serviceRouteDescriptor in serviceRouteDescriptors)
            {
                serviceRouteDescriptor.AddressDescriptors =
                    serviceRouteDescriptor.AddressDescriptors.Where(p =>
                        p != p.ConvertToAddressModel().Descriptor);
                await RegisterRouteWithLockAsync(serviceRouteDescriptor);
            }
        }
    }
}