using System.Linq;
using Silky.Rpc.Address;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServiceRoute
    {
        public IRpcAddress[] Addresses { get; set; }
        
        public ServiceDescriptor Service { get; set; }
        
        public override bool Equals(object? obj)
        {
            var model = obj as ServiceRoute;
            if (model == null)
            {
                return false;
            }
            if (!Service.Equals(model.Service))
            {
                return false;
            }
            return Addresses.All(p => model.Addresses.Any(q => p == q));
        }

        public static bool operator ==(ServiceRoute model1, ServiceRoute model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceRoute model1, ServiceRoute model2)
        {
            return !Equals(model1, model2);
        }

        public override int GetHashCode()
        {
            return (Service.ToString() + string.Join(",", Addresses.Select(p => p.ToString()))).GetHashCode();
        }
    }
}