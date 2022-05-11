using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Silky.Core.Exceptions;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.RegistryCenters.HeartBeat;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public class ConsulServerRegister : ServerRegisterBase
    {
        private readonly IConsulClientFactory _consulClientFactory;
        private readonly IServerConverter _serverConverter;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHeartBeatService _heartBeatService;

        public ConsulServerRegister(IServerManager serverManager,
            IServerProvider serverProvider,
            IConsulClientFactory consulClientFactory,
            IServerConverter serverConverter,
            IServiceProvider serviceProvider,
            IHeartBeatService heartBeatService)
            : base(serverManager, serverProvider)
        {
            _consulClientFactory = consulClientFactory;
            _serverConverter = serverConverter;
            _serviceProvider = serviceProvider;
            _heartBeatService = heartBeatService;
        }

        protected override async Task RemoveRpcEndpoint(string hostName, IRpcEndpoint rpcEndpoint)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            await consulClient.Agent.ServiceDeregister(rpcEndpoint.ToString());
        }

        protected override async Task CacheServers()
        {
            await CacheServersFromConsul();
            _heartBeatService.Start(CacheServersFromConsul);
        }

        private async Task CacheServersFromConsul()
        {
            using var consulClient = _consulClientFactory.CreateClient();
            var queryResult = await consulClient.Agent.Services();
            if (queryResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException("Cache Servers Error");
            }

            var allServerInstances =
                queryResult.Response.Values.Where(p => p.Tags.Contains(ConsulRegistryCenterOptions.SilkyServer));

            var allServerInfos = allServerInstances.GroupBy(p => p.GetServerName());

            foreach (var serverInfo in allServerInfos)
            {
                var serverDescriptor = await _serverConverter.Convert(serverInfo.Key, serverInfo.ToArray());
                _serverManager.Update(serverDescriptor);
            }
        }

        protected override async Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            var agentServiceRegistration = serverDescriptor.CreateAgentServiceRegistration();
            var serviceRegisterResult = await consulClient.Agent.ServiceRegister(agentServiceRegistration);
            if (serviceRegisterResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException("Register Server To ServiceCenter Consul Error");
            }

            await _serviceProvider.PublishAsync(serverDescriptor.HostName, serverDescriptor.Services);
        }

        protected override async Task RemoveServiceCenterExceptRpcEndpoint(IServer server)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            await consulClient.Agent.ServiceDeregister(server.GetInstanceId());
        }
    }
}