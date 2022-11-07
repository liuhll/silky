using System;
using System.Diagnostics;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Utils;

namespace Silky.Rpc.Endpoint.Descriptor
{
    public class SilkyEndpointDescriptor
    {
        public SilkyEndpointDescriptor()
        {
            ProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
        }

        public string Host { get; set; }

        public int Port { get; set; }

        public double ProcessorTime { get; set; }

        public long TimeStamp { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }

        public override bool Equals(object obj)
        {
            var model = obj as SilkyEndpointDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.ToString() == ToString() && model.ServiceProtocol == ServiceProtocol;
        }

        public override string ToString()
        {
            return string.Concat(ServiceProtocol.ToString().ToLower(), "://", SilkyEndpointHelper.GetIp(Host), ":",
                Port.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(SilkyEndpointDescriptor model1, SilkyEndpointDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(SilkyEndpointDescriptor model1, SilkyEndpointDescriptor model2)
        {
            return !Equals(model1, model2);
        }
    }
}