using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Address;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry;

namespace Lms.Rpc.Routing
{
    public interface IServiceRouteManager 
    {
        // Task SetRoutesAsync(IReadOnlyList<ServiceRouteDescriptor> serviceRouteDescriptors);
        
        Task RegisterRoutes(IReadOnlyList<ServiceEntry> localServiceEntries, AddressType address);
    }
}