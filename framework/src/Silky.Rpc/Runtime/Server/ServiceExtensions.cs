using System.Diagnostics;
using Silky.Core.Exceptions;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceExtensions
    {
        public static ServiceRouteDescriptor CreateLocalRouteDescriptor(this Service service,
            AddressDescriptor addressDescriptor)
        {
            if (!service.IsLocal)
            {
                throw new SilkyException("Only allow local service to generate routing descriptors");
            }

            return new ServiceRouteDescriptor()
            {
                ProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds,
                Service = service.ServiceDescriptor,
                Addresses = new[]
                    { addressDescriptor },
            };
        }
    }
}