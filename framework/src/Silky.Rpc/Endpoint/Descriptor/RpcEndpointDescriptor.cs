using System;
using System.Diagnostics;
using Silky.Core.Rpc;
using Silky.Core.Utils;

namespace Silky.Rpc.Endpoint.Descriptor
{
    public class RpcEndpointDescriptor
    {
        public RpcEndpointDescriptor()
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
            var model = obj as RpcEndpointDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.ToString() == ToString() && model.ServiceProtocol == ServiceProtocol;
        }

        public override string ToString()
        {
            return string.Concat(new[]
                { RpcEndpointHelper.GetIp(Host), ":", Port.ToString(), ":", ServiceProtocol.ToString() });
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(RpcEndpointDescriptor model1, RpcEndpointDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(RpcEndpointDescriptor model1, RpcEndpointDescriptor model2)
        {
            return !Equals(model1, model2);
        }
    }
}