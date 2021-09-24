using System.Collections.Generic;
using System.Linq;
using Consul;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.RegistryCenter.Consul.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Consul
{
    internal static class ServerDescriptorExtensions
    {
        public static AgentServiceRegistration CreateAgentServiceRegistration(this ServerDescriptor serverDescriptor)
        {
            var endpoint = serverDescriptor.Endpoints.FirstOrDefault(p =>
                p.ServiceProtocol == ServiceProtocol.Tcp || p.ServiceProtocol.IsHttp());

            if (endpoint == null)
            {
                throw new SilkyException("RpcEndpoint does not exist");
            }

            var serviceProtocols = new Dictionary<ServiceProtocol, int>();
            foreach (var rpcEndpointDescriptor in serverDescriptor.Endpoints)
            {
                serviceProtocols[rpcEndpointDescriptor.ServiceProtocol] =
                    rpcEndpointDescriptor.Port;
            }

            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var serviceProtocolJsonString = serializer.Serialize(serviceProtocols);
            var agentServiceRegistration = new AgentServiceRegistration()
            {
                ID = serverDescriptor.Endpoints.First().ToString(),
                Address = serverDescriptor.Endpoints.First().Host,
                Port = serverDescriptor.Endpoints.First().Port,
                Name = serverDescriptor.HostName,
                EnableTagOverride = false,
                Meta = new Dictionary<string, string>()
                {
                    { "ProcessorTime", endpoint.ProcessorTime.ToString() },
                    { "TimeStamp", endpoint.TimeStamp.ToString() },
                    { "ServiceProtocols", serviceProtocolJsonString },
                    { "HostName", EngineContext.Current.HostName }
                },
                Tags = new[]
                {
                    EngineContext.Current.HostName,
                    ConsulRegistryCenterOptions.SilkyServer
                },
                // Check = new AgentServiceCheck()
                // {
                //     HTTP = "",
                //     Method = "Post",
                //     Interval = TimeSpan.FromSeconds(5),
                //     DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5)
                // }
            };
            return agentServiceRegistration;
        }
    }
}