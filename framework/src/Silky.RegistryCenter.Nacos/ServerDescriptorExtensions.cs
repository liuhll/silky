using System.Collections.Generic;
using System.Linq;
using Nacos.V2.Naming.Dtos;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.RegistryCenter.Nacos
{
    public static class ServerDescriptorExtensions
    {
        public static Instance GetInstance(this ServerDescriptor serverDescriptor)
        {
            var endpoint =
                serverDescriptor.Endpoints.FirstOrDefault(p =>
                    p.ServiceProtocol == ServiceProtocol.Tcp || p.ServiceProtocol.IsHttp()
                );
            if (endpoint == null)
            {
                throw new SilkyException("RpcEndpoint does not exist");
            }

            var serviceProtocols = new Dictionary<ServiceProtocol, int>();
            foreach (var rpcEndpointDescriptor in serverDescriptor.Endpoints)
            {
                serviceProtocols[rpcEndpointDescriptor.ServiceProtocol] = rpcEndpointDescriptor.Port;
            }

            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var serviceProtocolJsonString = serializer.Serialize(serviceProtocols);
            var servicesJsonString = serializer.Serialize(serverDescriptor.Services);
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
                    { "ServiceProtocols", serviceProtocolJsonString },
                    { "Services", servicesJsonString },
                }
            };
            return instance;
        }
    }
}