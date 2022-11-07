using System;
using Silky.Core;
using Silky.Core.Runtime.Rpc;

namespace Silky.Rpc.Endpoint.Descriptor
{
    public static class SilkyEndpointDescriptorExtensions
    {
        public static ISilkyEndpoint ConvertToSilkyEndpoint(this SilkyEndpointDescriptor silkyEndpointDescriptor)
        {
            if (!SingletonDictionary<string, ISilkyEndpoint>.Instance.ContainsKey(silkyEndpointDescriptor.ToString()))
            {
                SingletonDictionary<string, ISilkyEndpoint>.Instance[silkyEndpointDescriptor.ToString()] =
                    Singleton<ISilkyEndpoint>.Instance ?? new SilkyEndpoint(silkyEndpointDescriptor.Host,
                        silkyEndpointDescriptor.Port, silkyEndpointDescriptor.ServiceProtocol);
            }

            SingletonDictionary<string, ISilkyEndpoint>.Instance[silkyEndpointDescriptor.ToString()].InitFuseTimes();
            return SingletonDictionary<string, ISilkyEndpoint>.Instance[silkyEndpointDescriptor.ToString()];
        }

        public static string GetAddress(this SilkyEndpointDescriptor silkyEndpointDescriptor)
        {
            var address = $"{silkyEndpointDescriptor.Host}:{silkyEndpointDescriptor.Port}";
            return address;
        }

        public static bool IsInstanceEndpoint(this SilkyEndpointDescriptor silkyEndpointDescriptor)
        {
            return silkyEndpointDescriptor.ServiceProtocol == ServiceProtocol.Rpc;
        }
    }
}