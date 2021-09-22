using System;
using System.Linq;
using Silky.Core.Utils;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing.Descriptor
{
    public class ServerRouteDescriptor 
    {
        public ServerRouteDescriptor()
        {
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
            Services = Array.Empty<ServiceDescriptor>();
            Endpoints = Array.Empty<RpcEndpointDescriptor>();
        }

        public string HostName { get; set; }

        public ServiceDescriptor[] Services { get; set; }

        public RpcEndpointDescriptor[] Endpoints { get; set; }

        public long TimeStamp { get; set; }

        public override bool Equals(object? obj)
        {
            var model = obj as ServerRouteDescriptor;
            if (model == null)
            {
                return false;
            }

            return Services.All(p => model.Services.Any(q => p == q))
                   && Endpoints.All(p => model.Endpoints.Any(q => p == q));
        }

        public static bool operator ==(ServerRouteDescriptor model1, ServerRouteDescriptor model2)
        {
            return Equals(model1, model2);
        }
        
        public static bool operator !=(ServerRouteDescriptor model1, ServerRouteDescriptor model2)
        {
            return !Equals(model1, model2);
        }

        public override int GetHashCode()
        {
            return (string.Join("", Services.Select(p => p.ToString())) +
                    string.Join("", Endpoints.Select(p => p.ToString()))).GetHashCode();
        }
        
    }
}