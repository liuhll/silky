using System.Collections.Generic;
using System.Linq;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public class ServerRoute
    {
        public ServerRoute(string hostName)
        {
            HostName = hostName;
            Endpoints = new List<IRpcEndpoint>();
            Services = new List<ServiceDescriptor>();
        }

        public string HostName { get; }
        public ICollection<IRpcEndpoint> Endpoints { get; set; }
        public ICollection<ServiceDescriptor> Services { get; set; }

        public override bool Equals(object? obj)
        {
            var model = obj as ServerRoute;
            if (model == null)
            {
                return false;
            }

            if (!HostName.Equals(model.HostName))
            {
                return false;
            }

            return Services.All(p => model.Services.Any(q => p == q))
                   && Endpoints.All(p => model.Endpoints.Any(q => p == q));
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
            return (string.Join(",", Services.Select(p => p.ToString())) +
                    string.Join(",", Endpoints.Select(p => p.ToString()))).GetHashCode();
        }
    }
}