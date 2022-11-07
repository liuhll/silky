using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nacos.V2;
using Nacos.V2.Naming.Dtos;
using Silky.RegistryCenter.Nacos.Configuration;
using Silky.RegistryCenter.Nacos.Listeners;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;


namespace Silky.RegistryCenter.Nacos
{
    public class NacosServerRegister : ServerRegisterBase
    {
        private readonly INacosNamingService _nacosNamingService;
        private readonly NacosRegistryCenterOptions _nacosRegistryCenterOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerRegisterProvider _serverRegisterProvider;

        private ConcurrentDictionary<string, ServerListener> m_serverListeners = new();


        public NacosServerRegister(IServerManager serverManager,
            IServerProvider serverProvider,
            IServiceProvider serviceProvider,
            INacosNamingService nacosNamingService,
            IOptionsMonitor<NacosRegistryCenterOptions> nacosRegistryCenterOptions,
            IServerRegisterProvider serverRegisterProvider)
            : base(serverManager,
                serverProvider)
        {
            _nacosNamingService = nacosNamingService;
            _serverRegisterProvider = serverRegisterProvider;
            _serviceProvider = serviceProvider;
            _nacosRegistryCenterOptions = nacosRegistryCenterOptions.CurrentValue;
        }

        protected override async Task RemoveRpcEndpoint(string hostName, ISilkyEndpoint silkyEndpoint)
        {
            var serverInstances = await _nacosNamingService.GetAllInstances(hostName);

            var unHealthInstance = serverInstances.FirstOrDefault(p => p.ServiceName == hostName
                                                                       && p.Ip == silkyEndpoint.Host
                                                                       && p.GetServiceProtocolInfos()
                                                                           .ContainsKey(silkyEndpoint.ServiceProtocol));
            if (unHealthInstance != null)
            {
                await _nacosNamingService.DeregisterInstance(hostName, unHealthInstance);
            }
        }

        protected override async Task CacheServers()
        {
            var serverNames = await _serverRegisterProvider.GetAllServerNames();
            foreach (var serverName in serverNames)
            {
                await CreateServerListener(serverName);
            }
        }

        internal async Task CreateServerListener(string serverName)
        {
            if (!m_serverListeners.ContainsKey(serverName))
            {
                var serverListener = new ServerListener(this);
                m_serverListeners.TryAdd(serverName, serverListener);
                await _nacosNamingService.Subscribe(serverName, _nacosRegistryCenterOptions.ServerGroupName, serverListener);
                var serverInstances = await _nacosNamingService.GetAllInstances(serverName);
                await UpdateServer(serverName, serverInstances);
            }
        }

        internal async Task UpdateServer(string serverName, List<Instance> instances)
        {
            var services = await _serviceProvider.GetServices(serverName);
            var serverDescriptor =
                _serverRegisterProvider.GetServerDescriptor(serverName, instances, services);
            _serverManager.Update(serverDescriptor);
        }

        protected override async Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor)
        {
            await _serviceProvider.PublishServices(serverDescriptor.HostName, serverDescriptor.Services);
            var instance = serverDescriptor.GetInstance();
            await _nacosNamingService.RegisterInstance(
                serverDescriptor.HostName,
                _nacosRegistryCenterOptions.ServerGroupName,
                instance);
            await _serverRegisterProvider.AddServer();
        }


        protected override async Task RemoveServiceCenterExceptRpcEndpoint(IServer server)
        {
        }
    }
}