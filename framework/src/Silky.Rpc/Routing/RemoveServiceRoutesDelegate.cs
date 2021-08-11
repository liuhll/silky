using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Address;
using Silky.Rpc.Routing.Descriptor;

namespace Silky.Rpc.Routing
{
    public delegate Task OnRemoveServiceRoutes(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors,
        IAddressModel addressModel);

    public delegate Task OnRemoveServiceRoute(string serviceId, IAddressModel addressModel);
}