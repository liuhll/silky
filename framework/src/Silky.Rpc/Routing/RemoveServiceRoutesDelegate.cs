using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Routing.Descriptor;

namespace Silky.Rpc.Routing
{
    public delegate Task OnRemoveServiceRoutes(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors);
}