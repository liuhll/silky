using System.Collections.Generic;
using Lms.Rpc.Address.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;

namespace Lms.Rpc.Routing.Descriptor
{
    public class ServiceRouteDescriptor
    {
        public ServiceDescriptor ServiceDescriptor { get; set; }

        public IEnumerable<AddressDescriptor> AddressDescriptors { get; set; }
    }
}