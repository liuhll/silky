using System.Collections.Generic;
using Lms.Rpc.Address;
using Lms.Rpc.Runtime.Server.Descriptor;

namespace Lms.Rpc.Routing
{
    public class ServiceRoute
    {
        public IAddressModel[] Addresses { get; set; }
        
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}