using Silky.Rpc.Address;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServiceRoute
    {
        public string HostName { get; set; }
        public IAddressModel[] Addresses { get; set; }
        
        public ServiceDescriptor[] Services { get; set; }
    }
}