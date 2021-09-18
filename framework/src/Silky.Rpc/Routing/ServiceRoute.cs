using Silky.Rpc.Address;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServiceRoute
    {
        public IRpcAddress[] Addresses { get; set; }
        
        public ServiceDescriptor Service { get; set; }
    }
}