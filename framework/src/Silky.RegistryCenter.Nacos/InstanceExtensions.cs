using System.Collections.Generic;
using Nacos.V2.Naming.Dtos;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.RegistryCenter.Nacos
{
    internal static class InstanceExtensions
    {
        public static IEnumerable<SilkyEndpointDescriptor> GetEndpoints(this Instance instance)
        {
            var endpoints = new List<SilkyEndpointDescriptor>();
            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var endpointsJsonString = instance.Metadata["Endpoints"];
            var endpointInfos =
                serializer.Deserialize<Dictionary<ServiceProtocol, string>>(endpointsJsonString);
            foreach (var endpointInfo in endpointInfos)
            {
                var rpcEndpointDescriptor = new SilkyEndpointDescriptor()
                {
                    Host = endpointInfo.Value.Split(":")[0],
                    Port = endpointInfo.Value.Split(":")[1].To<int>(),
                    ServiceProtocol = endpointInfo.Key,
                    TimeStamp = instance.Metadata["TimeStamp"].To<long>(),
                    ProcessorTime = instance.Metadata["ProcessorTime"].To<double>(),
                };
                endpoints.Add(rpcEndpointDescriptor);
            }

            return endpoints;
        }

        public static string GetServerName(this Instance instance)
        {
            return instance.Metadata["HostName"];
        }

        public static Dictionary<ServiceProtocol, string> GetServiceProtocolInfos(this Instance instance)
        {
            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var endpointsJsonString = instance.Metadata["Endpoints"];
            var serviceProtocolInfos =
                serializer.Deserialize<Dictionary<ServiceProtocol, string>>(endpointsJsonString);
            return serviceProtocolInfos;
        }
    }
}