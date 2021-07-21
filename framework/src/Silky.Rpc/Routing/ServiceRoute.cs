using System.Collections.Generic;
using Silky.Rpc.Address;
using Silky.Rpc.Runtime.Server.Descriptor;

namespace Silky.Rpc.Routing
{
    public class ServiceRoute
    {
        public IAddressModel[] Addresses { get; set; }
        
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}