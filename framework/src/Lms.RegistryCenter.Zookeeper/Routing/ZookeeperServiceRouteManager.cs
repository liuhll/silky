using System;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.DependencyInjection;
using Lms.Core.Serialization;
using Lms.Rpc.Configuration;
using Lms.Rpc.Routing;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry;
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
        private readonly ILogger<ZookeeperServiceRouteManager> _logger;
        private readonly IObjectSerializer _objectSerializer;

        public ZookeeperServiceRouteManager(ServiceRouteCache serviceRouteCache,
            IServiceEntryManager serviceEntryManager,
            IZookeeperClientProvider zookeeperClientProvider, IOptions<RegistryCenterOptions> registryCenterOptions,
            IObjectSerializer objectSerializer)
            : base(serviceRouteCache, serviceEntryManager, registryCenterOptions)
        {
            _zookeeperClientProvider = zookeeperClientProvider;
            _objectSerializer = objectSerializer;
            _logger = NullLogger<ZookeeperServiceRouteManager>.Instance;
            EnterRoutes().GetAwaiter().GetResult();
        }

        protected async Task EnterRoutes()
        {
            var zookeeperClient = _zookeeperClientProvider.GetZooKeeperClient();

            if (await zookeeperClient.ExistsAsync(_registryCenterOptions.RoutePath))
            {
                var children = await zookeeperClient.GetChildrenAsync(_registryCenterOptions.RoutePath);
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
            var routeDescriptor = _objectSerializer.Deserialize<ServiceRouteDescriptor>(data.ToArray());
            return routeDescriptor;
        }

        private string CreateRoutePath(string child)
        {
            var routePath = _registryCenterOptions.RoutePath;
            if (!_registryCenterOptions.RoutePath.EndsWith("/"))
            {
                routePath += "/";
            }

            routePath += child;
            return routePath;
        }

        protected async override Task RegisterRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor)
        {
            var zookeeperClients = _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                var routePath = CreateRoutePath(serviceRouteDescriptor.ServiceDescriptor.Id);
                var data = _objectSerializer.Serialize(serviceRouteDescriptor);
                if (!await zookeeperClient.ExistsAsync(routePath))
                {
                    _logger.LogDebug($"节点：{routePath}不存在将进行创建");
                    await zookeeperClient.CreateAsync(routePath, data, ZooDefs.Ids.OPEN_ACL_UNSAFE,
                        CreateMode.PERSISTENT);
                }
                else
                {
                    var onlineData = (await zookeeperClient.GetDataAsync(routePath)).ToArray();
                    // todo 比较数据是否一致
                    await zookeeperClient.SetDataAsync(routePath, data);
                    _logger.LogDebug($"{routePath}节点的缓存的服务路由与服务注册中心不一致,路由数据已被更新。");
                }
            }
        }
    }
}