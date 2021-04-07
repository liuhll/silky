using Newtonsoft.Json;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Rpc.Address.Descriptor
{
    public class AddressDescriptor
    {
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