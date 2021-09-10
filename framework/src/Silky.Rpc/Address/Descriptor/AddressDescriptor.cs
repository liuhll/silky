using System;
using System.Diagnostics;
using Silky.Core.Utils;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Address.Descriptor
{
    public class AddressDescriptor
    {
        public AddressDescriptor()
        {
            ProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
            TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
        }

        public double ProcessorTime { get; set; }
        
        public long TimeStamp { get; set; }
        
        public string Address { get; set; }

        public int Port { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }
        
        public override bool Equals(object obj)
        {
            var model = obj as AddressDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.ToString() == ToString() && model.ServiceProtocol == ServiceProtocol;
        }

        public override string ToString()
        {
            return string.Concat(new[] {AddressHelper.GetIp(Address), ":", Port.ToString(), ":", ServiceProtocol.ToString()});
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(AddressDescriptor model1, AddressDescriptor model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(AddressDescriptor model1, AddressDescriptor model2)
        {
            return !Equals(model1, model2);
        }
    }
}