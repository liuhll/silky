using System.Collections.Generic;
using System.Threading.Tasks;
using Nacos.V2;
using Nacos.V2.Naming.Dtos;
using Silky.Core;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public class NacosServerRegister : ServerRegisterBase
    {
        private readonly INacosNamingService _nacosNamingService;

        public NacosServerRegister(IServerManager serverManager,
            IServerProvider serverProvider,
            INacosNamingService nacosNamingService)
            : base(serverManager,
                serverProvider)
        {
            _nacosNamingService = nacosNamingService;
        }

        public async override Task RemoveSelfServer()
        {
        }

        protected override async Task RemoveRpcEndpoint(string hostName, IRpcEndpoint rpcEndpoint)
        {
        }

        protected override async Task CacheServers()
        {
            var allInstances = await _nacosNamingService.GetAllInstances(EngineContext.Current.HostName);
        }

        protected override async Task RegisterServerToServiceCenter(ServerDescriptor serverDescriptor)
        {
            foreach (var endpoint in serverDescriptor.Endpoints)
            {
                var instance = new Instance()
                {
                    InstanceId = endpoint.ToString(),
                    ServiceName = serverDescriptor.HostName,
                    Ephemeral = true,
                    Enabled = true,
                    Ip = endpoint.Host,
                    Port = endpoint.Port,
                    Metadata = new Dictionary<string, string>()
                    {
                        { "ServiceProtocol", endpoint.ServiceProtocol.ToString() },
                        { "TimeStamp", serverDescriptor.TimeStamp.ToString() },
                        { "ProcessorTime", endpoint.ProcessorTime.ToString() }
                    },
                };
                await _nacosNamingService.RegisterInstance(serverDescriptor.HostName, instance);
            }
        }

        protected override async Task RemoveServiceCenterExceptRpcEndpoint(IServer server)
        {
        }
    }
}