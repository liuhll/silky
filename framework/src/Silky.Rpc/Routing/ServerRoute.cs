using System.Linq;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServerRoute
    {
        public IRpcEndpoint[] Endpoints { get; set; }
        
        public ServiceDescriptor Service { get; set; }
        
        public override bool Equals(object? obj)
        {
            var model = obj as ServerRoute;
            if (model == null)
            {
                return false;
            }
            if (!Service.Equals(model.Service))
            {
                return false;
            }
            return Endpoints.All(p => model.Endpoints.Any(q => p == q));
        }

        public static bool operator ==(ServerRoute model1, ServerRoute model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServerRoute model1, ServerRoute model2)
        {
            return !Equals(model1, model2);
        }

        public override int GetHashCode()
        {
            return (Service.ToString() + string.Join(",", Endpoints.Select(p => p.ToString()))).GetHashCode();
        }
    }
}