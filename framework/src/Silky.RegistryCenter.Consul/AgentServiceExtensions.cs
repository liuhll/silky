using System.Collections.Generic;
using Consul;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.RegistryCenter.Consul
{
    public static class AgentServiceExtensions
    {
        public static SilkyEndpointDescriptor[] GetEndpointDescriptors(this AgentService agentService)
        {
            var serviceProtocols = agentService.Meta["ServiceProtocols"];
            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var serviceProtocolInfos = serializer.Deserialize<Dictionary<ServiceProtocol, int>>(serviceProtocols);
            var rpcEndpointDescriptors = new List<SilkyEndpointDescriptor>();
            foreach (var serviceProtocolInfo in serviceProtocolInfos)
            {
                var rpcEndpointDescriptor = new SilkyEndpointDescriptor()
                {
                    Host = agentService.Address,
                    Port = serviceProtocolInfo.Value,
                    ServiceProtocol = serviceProtocolInfo.Key,
                    TimeStamp = agentService.Meta["TimeStamp"].To<long>(),
                    ProcessorTime = agentService.Meta["ProcessorTime"].To<double>(),
                };
                if (serviceProtocolInfo.Key.IsHttp() && agentService.Meta["HttpHost"] != null)
                {
                    rpcEndpointDescriptor.Host = agentService.Meta["HttpHost"];
                }

                rpcEndpointDescriptors.Add(rpcEndpointDescriptor);
            }

            return rpcEndpointDescriptors.ToArray();
        }

        public static string GetServerName(this AgentService agentService)
        {
            return agentService.Meta["HostName"];
        }
    }
}