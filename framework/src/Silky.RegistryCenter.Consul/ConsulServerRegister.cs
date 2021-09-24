using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    public class ConsulServerRegister : ServerRegisterBase
    {
        private readonly IConsulClientFactory _consulClientFactory;
        private readonly ISerializer _serializer;

        public ConsulServerRegister(IServerManager serverManager,
            IServerProvider serverProvider,
            IConsulClientFactory consulClientFactory,
            ISerializer serializer)
            : base(serverManager, serverProvider)
        {
            _consulClientFactory = consulClientFactory;
            _serializer = serializer;
        }

        protected override async Task RemoveRpcEndpoint(string hostName, IRpcEndpoint rpcEndpoint)
        {
        }

        protected override async Task CacheServers()
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

            foreach (var allServerInfo in allServerInfos)
            {
                // if (!serverInfo.Value.Tags.Contains(ConsulRegistryCenterOptions.SilkyServer))
                // {
                //     continue;
                // }
                //
                // var serverDescriptor = serverInfo.Value.GetServerDescriptor();
                // _serverManager.Update(serverDescriptor);
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

            var servicesJsonString = _serializer.Serialize(serverDescriptor.Services);
            var servicesPutResult = await consulClient.KV.Put(new KVPair($"services/{EngineContext.Current.HostName}")
            {
                Value = servicesJsonString.GetBytes()
            });
            if (servicesPutResult.StatusCode != HttpStatusCode.OK)
            {
                throw new SilkyException("Register Server To ServiceCenter Consul Error");
            }

        }

        protected override async Task RemoveServiceCenterExceptRpcEndpoint(IServer server)
        {
        }
    }
}