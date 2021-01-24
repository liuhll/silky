using System.Collections.Generic;
using Lms.Rpc.Address;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;

namespace Lms.Rpc.Routing
{
    public class ServiceRoute
    {
        public IEnumerable<IAddressModel> Addresses { get; set; }
        
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}