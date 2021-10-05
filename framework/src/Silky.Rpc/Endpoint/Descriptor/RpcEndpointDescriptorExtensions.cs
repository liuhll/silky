using System;
using Silky.Core;
using Silky.Core.Rpc;

namespace Silky.Rpc.Endpoint.Descriptor
{
    public static class RpcEndpointDescriptorExtensions
    {
        public static IRpcEndpoint ConvertToRpcEndpoint(this RpcEndpointDescriptor rpcEndpointDescriptor)
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
            var address = $"{rpcEndpointDescriptor.Host}:{rpcEndpointDescriptor.Port}";
            return address;
        }

        public static bool IsInstanceEndpoint(this RpcEndpointDescriptor rpcEndpointDescriptor)
        {
            return rpcEndpointDescriptor.ServiceProtocol == ServiceProtocol.Tcp;
        }
    }
}