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
            var serverListener = new ServerListener(this);
            await _nacosNamingService.Subscribe(_nacosRegistryCenterOptions.ServiceName,
                _nacosRegistryCenterOptions.ServerGroupName,
                serverListener);
        }


        internal async Task UpdateServer(string serviceName, string groupName, List<Instance> instances)
        {
            if (serviceName.Equals(_nacosRegistryCenterOptions.ServiceName) &&
                groupName.Equals(_nacosRegistryCenterOptions.ServerGroupName))
            {
                var serverDescriptors =
                    await _serverRegisterProvider.GetServerDescriptor(instances);
                foreach (var serverDescriptor in serverDescriptors)
                {
                    _serverManager.Update(serverDescriptor);
                }
            }
        }

        protected override async Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor)
        {
            await _serviceProvider.PublishServices(serverDescriptor.HostName, serverDescriptor.Services);
            var instance = serverDescriptor.GetInstance();

            await _nacosNamingService.RegisterInstance(
                _nacosRegistryCenterOptions.ServiceName,
                _nacosRegistryCenterOptions.ServerGroupName,
                instance);
        }


        protected override async Task RemoveServiceCenterExceptRpcEndpoint(IServer server)
        {
        }
    }
}