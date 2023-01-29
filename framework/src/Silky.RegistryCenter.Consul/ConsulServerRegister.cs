using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.RegistryCenter.Consul.HealthCheck;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Extensions;
using Silky.Rpc.RegistryCenters.HeartBeat;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public class ConsulServerRegister : ServerRegisterBase
    {
        private readonly IConsulClientFactory _consulClientFactory;
        private readonly IServerConverter _serverConverter;
        private readonly IServiceDescriptorProvider _serviceDescriptorProvider;
        private readonly IHeartBeatService _heartBeatService;
        private readonly IHealthCheckService _healthCheckService;
        private ConsulRegistryCenterOptions _consulRegistryCenterOptions;

        public ConsulServerRegister(IServerManager serverManager,
            IServerProvider serverProvider,
            IConsulClientFactory consulClientFactory,
            IServerConverter serverConverter,
            IServiceDescriptorProvider serviceDescriptorProvider,
            IHeartBeatService heartBeatService,
            IHealthCheckService healthCheckService,
            IOptions<ConsulRegistryCenterOptions> consulRegistryCenterOptions)
            : base(serverManager, serverProvider)
        {
            _consulClientFactory = consulClientFactory;
            _serverConverter = serverConverter;
            _serviceDescriptorProvider = serviceDescriptorProvider;
            _heartBeatService = heartBeatService;
            _healthCheckService = healthCheckService;
            _consulRegistryCenterOptions = consulRegistryCenterOptions.Value;
        }

        protected override async Task RemoveSilkyEndpoint(string hostName, ISilkyEndpoint silkyEndpoint)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            var serviceId = silkyEndpoint.GetAddress();
            var result = await consulClient.Agent.ServiceDeregister(serviceId);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning($"remove serviceId {serviceId} fail");
            }
        }

        protected override async Task CacheServers()
        {
            await CacheServersFromConsul(false);
            _heartBeatService.Start(HeartBeatServers);
        }

        private async Task HeartBeatServers()
        {
            if (!await RepeatRegister())
            {
                await CacheServersFromConsul(true);
            }
        }

        private async Task CacheServersFromConsul(bool isHeartBeat)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            var queryResult = await consulClient.Agent.Services();
            if (queryResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException("Cache Servers Error");
            }

            var allServerInstances =
                queryResult.Response.Values.Where(p => p.Tags.Contains(ConsulRegistryCenterOptions.SilkyServer));

            var allServerInfos = allServerInstances.GroupBy(p => p.Service);


            var serverDescriptors = new List<ServerDescriptor>();
            foreach (var serverInfo in allServerInfos)
            {
                var serverInstances = serverInfo.ToList();
                if (_consulRegistryCenterOptions.HealthCheck && isHeartBeat &&
                    serverInfo.All(s => bool.Parse(s.Meta["HealthCheck"])))
                {
                    var unHealthServiceIds = await _healthCheckService.Check(consulClient, serverInfo.Key);
                    serverInstances = serverInstances.Where(s => !unHealthServiceIds.Contains(s.ID)).ToList();
                }

                var serverDescriptor = await _serverConverter.Convert(serverInfo.Key, serverInstances.ToArray());
                serverDescriptors.Add(serverDescriptor);
            }

            _serverManager.UpdateAll(serverDescriptors.ToArray());
        }

        protected override async Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            var agentServiceRegistration =
                serverDescriptor.CreateAgentServiceRegistration(_consulRegistryCenterOptions);
            var serviceRegisterResult = await consulClient.Agent.ServiceRegister(agentServiceRegistration);
            if (serviceRegisterResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException($"Register {serverDescriptor.HostName} Server To ServiceCenter Consul Error");
            }

            await _serviceDescriptorProvider.PublishAsync(serverDescriptor.HostName, serverDescriptor.Services);
        }

        protected override async Task RemoveServiceCenterExceptRpcEndpoint(IServer server)
        {
            using var consulClient = _consulClientFactory.CreateClient();
            await consulClient.Agent.ServiceDeregister(server.GetInstanceId());
        }
    }
}