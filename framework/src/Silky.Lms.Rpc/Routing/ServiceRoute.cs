using System.Collections.Generic;
using Silky.Lms.Rpc.Address;
using Silky.Lms.Rpc.Runtime.Server.Descriptor;

namespace Silky.Lms.Rpc.Routing
{
    public class ServiceRoute
    {
        public IAddressModel[] Addresses { get; set; }
        
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}