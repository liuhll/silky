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
            var endpoints = agentService.Meta["Endpoints"];
            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var endpointInfos = serializer.Deserialize<Dictionary<ServiceProtocol, string>>(endpoints);
            var rpcEndpointDescriptors = new List<SilkyEndpointDescriptor>();
            foreach (var endpointInfo in endpointInfos)
            {
                var rpcEndpointDescriptor = new SilkyEndpointDescriptor()
                {
                    Host = endpointInfo.Value.Split(":")[0],
                    Port = endpointInfo.Value.Split(":")[1].To<int>(),
                    ServiceProtocol = endpointInfo.Key,
                    TimeStamp = agentService.Meta["TimeStamp"].To<long>(),
                    ProcessorTime = agentService.Meta["ProcessorTime"].To<double>(),
                };
                rpcEndpointDescriptors.Add(rpcEndpointDescriptor);
            }

            return rpcEndpointDescriptors.ToArray();
        }
        
    }
}