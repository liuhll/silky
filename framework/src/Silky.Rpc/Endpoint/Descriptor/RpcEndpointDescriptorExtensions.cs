using System;
using Silky.Core;
using Silky.Core.Rpc;

namespace Silky.Rpc.Endpoint.Descriptor
{
    public static class RpcEndpointDescriptorExtensions
    {
        public static IRpcEndpoint ConvertToAddressModel(this RpcEndpointDescriptor rpcEndpointDescriptor)
        {
            if (!SingletonDictionary<string, IRpcEndpoint>.Instance.ContainsKey(rpcEndpointDescriptor.ToString()))
            {
                SingletonDictionary<string, IRpcEndpoint>.Instance[rpcEndpointDescriptor.ToString()] =
                    Singleton<IRpcEndpoint>.Instance ?? new RpcEndpoint(rpcEndpointDescriptor.Host,
                        rpcEndpointDescriptor.Port, rpcEndpointDescriptor.ServiceProtocol);
            }

            SingletonDictionary<string, IRpcEndpoint>.Instance[rpcEndpointDescriptor.ToString()].InitFuseTimes();
            return SingletonDictionary<string, IRpcEndpoint>.Instance[rpcEndpointDescriptor.ToString()];
        }

        public static string GetHostAddress(this RpcEndpointDescriptor rpcEndpointDescriptor)
        {
            string address = String.Empty;
            switch (rpcEndpointDescriptor.ServiceProtocol)
            {
                case ServiceProtocol.Http:
                case ServiceProtocol.Https:
                case ServiceProtocol.Ws:
                case ServiceProtocol.Wss:
                case ServiceProtocol.Mqtt:
                    address =
                        $"{rpcEndpointDescriptor.ServiceProtocol.ToString().ToLower()}://{rpcEndpointDescriptor.Host}:{rpcEndpointDescriptor.Port}";
                    break;
                case ServiceProtocol.Tcp:
                    address =
                        $"{rpcEndpointDescriptor.Host}:{rpcEndpointDescriptor.Port}";
                    break;
            }

            return address;
        }
    }
}