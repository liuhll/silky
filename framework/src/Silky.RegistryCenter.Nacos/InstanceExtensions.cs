using System.Collections.Generic;
using Nacos.V2.Naming.Dtos;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.RegistryCenter.Nacos
{
    public static class InstanceExtensions
    {
        public static IEnumerable<SilkyEndpointDescriptor> GetEndpoints(this Instance instance)
        {
            var endpoints = new List<SilkyEndpointDescriptor>();
            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var serviceProtocolJsonString = instance.Metadata["ServiceProtocols"];
            var serviceProtocolInfos =
                serializer.Deserialize<Dictionary<ServiceProtocol, int>>(serviceProtocolJsonString);
            foreach (var serviceProtocolInfo in serviceProtocolInfos)
            {
                var rpcEndpointDescriptor = new SilkyEndpointDescriptor()
                {
                    Host = instance.Ip,
                    Port = serviceProtocolInfo.Value,
                    ServiceProtocol = serviceProtocolInfo.Key,
                    TimeStamp = instance.Metadata["TimeStamp"].To<long>(),
                    ProcessorTime = instance.Metadata["ProcessorTime"].To<double>(),
                };
                if (serviceProtocolInfo.Key.IsHttp() && instance.Metadata["HttpHost"] != null)
                {
                    rpcEndpointDescriptor.Host = instance.Metadata["HttpHost"];
                }

                endpoints.Add(rpcEndpointDescriptor);
            }

            return endpoints;
        }

        public static Dictionary<ServiceProtocol, int> GetServiceProtocolInfos(this Instance instance)
        {
            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var serviceProtocolJsonString = instance.Metadata["ServiceProtocols"];
            var serviceProtocolInfos =
                serializer.Deserialize<Dictionary<ServiceProtocol, int>>(serviceProtocolJsonString);
            return serviceProtocolInfos;
        }
    }
}