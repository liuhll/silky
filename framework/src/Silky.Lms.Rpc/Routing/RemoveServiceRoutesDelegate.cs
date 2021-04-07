using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Lms.Rpc.Address.Descriptor;
using Silky.Lms.Rpc.Routing.Descriptor;

namespace Silky.Lms.Rpc.Routing
{
    public delegate Task OnRemoveServiceRoutes(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors);
}