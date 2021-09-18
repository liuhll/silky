using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Core.Utils;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing.Descriptor
{
    public class ServiceRouteDescriptor
    {
        public ServiceRouteDescriptor()
        {
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
            Addresses = new List<RpcEndpointDescriptor>();
        }

        public ServiceDescriptor Service { get; set; }
        
        public IEnumerable<RpcEndpointDescriptor> Addresses { get; set; }

        public long TimeStamp { get; set; }

        public override bool Equals(object? obj)
        {
            var model = obj as ServiceRouteDescriptor;
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

        public static bool operator ==(ServiceRouteDescriptor model1, ServiceRouteDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceRouteDescriptor model1, ServiceRouteDescriptor model2)
        {
            return !Equals(model1, model2);
        }

        public override int GetHashCode()
        {
            return (Service.ToString() + string.Join(",", Addresses.Select(p => p.ToString()))).GetHashCode();
        }
    }
}