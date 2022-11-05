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
            Endpoints = Array.Empty<SilkyEndpointDescriptor>();
        }

        public string HostName { get; set; }

        public ServiceDescriptor[] Services { get; set; }

        public SilkyEndpointDescriptor[] Endpoints { get; set; }

        public long TimeStamp { get; set; }

        public override bool Equals(object? obj)
        {
            var model = obj as ServerDescriptor;
            if (model == null)
            {
                return false;
            }
            
            if (!HostName.Equals(model.HostName))
            {
                return false;
            }


            return Services.Length == model.Services.Length
                   && Services.All(p => model.Services.Any(p.Equals))
                   && Endpoints.Length == model.Endpoints.Length
                   && Endpoints.All(p => model.Endpoints.Any(p.Equals));
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