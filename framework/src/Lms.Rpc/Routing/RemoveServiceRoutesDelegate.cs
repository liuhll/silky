using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Address.Descriptor;
using Lms.Rpc.Routing.Descriptor;

namespace Lms.Rpc.Routing
{
    public delegate Task OnRemoveServiceRoutes(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors);
}