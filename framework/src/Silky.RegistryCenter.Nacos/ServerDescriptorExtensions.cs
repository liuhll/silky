using System.Collections.Generic;
using System.Linq;
using Nacos.V2.Naming.Dtos;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public static class ServerDescriptorExtensions
    {
        
        public static Instance GetInstance(this ServerDescriptor serverDescriptor)
        {
            var endpoint = serverDescriptor.GetRegisterEndpoint();
           

            var endpoints = new Dictionary<ServiceProtocol, string>();
            foreach (var rpcEndpointDescriptor in serverDescriptor.Endpoints)
            {
                endpoints[rpcEndpointDescriptor.ServiceProtocol] = rpcEndpointDescriptor.GetAddress();
            }

            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var endpointJsonString = serializer.Serialize(endpoints);
            var instance = new Instance()
            {
                Ip = endpoint.Host,
                Port = endpoint.Port,
                InstanceId = endpoint.ToString(),
                ServiceName = serverDescriptor.HostName,
                Metadata = new Dictionary<string, string>()
                {
                    { "ProcessorTime", endpoint.ProcessorTime.ToString() },
                    { "TimeStamp", endpoint.TimeStamp.ToString() },
                    { "Endpoints", endpointJsonString },
                    { "HostName", EngineContext.Current.HostName }
                }
            };
            return instance;
        }
    }
}