using Silky.Rpc.Address;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServiceRoute
    {
        public IAddressModel[] Addresses { get; set; }
        
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}