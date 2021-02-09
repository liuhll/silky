using System.Collections.Generic;
using Lms.Rpc.Address;
using Lms.Rpc.Runtime.Server.Descriptor;

namespace Lms.Rpc.Routing
{
    public class ServiceRoute
    {
        public IEnumerable<IAddressModel> Addresses { get; set; } = new List<IAddressModel>();
        
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}