using System;
using System.Linq;
using Silky.Core.Utils;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerDescriptor
    {
        public ServerDescriptor()
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
            var model = obj as ServerDescriptor;
            if (model == null)
            {
                return false;
            }

            return Services.All(p => model.Services.Any(q => p == q))
                   && Endpoints.All(p => model.Endpoints.Any(q => p == q));
        }

        public static bool operator ==(ServerDescriptor model1, ServerDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServerDescriptor model1, ServerDescriptor model2)
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