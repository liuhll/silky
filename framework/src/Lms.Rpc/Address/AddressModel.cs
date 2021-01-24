using System.Diagnostics.CodeAnalysis;
using System.Net;
using Lms.Core;
using Lms.Rpc.Address.Descriptor;

namespace Lms.Rpc.Address
{
    public class AddressModel : IAddressModel
    {
        public AddressModel([NotNull]string address, [NotNull]int port, AddressType addressType = AddressType.Rpc)
        {
            Check.NotNull(address,nameof(address));
            Address = address;
            Port = port;
            AddressType = addressType;
            Descriptor = new AddressDescriptor() {Address = Address, Port = Port, AddressType = AddressType};
        }

        public string Address { get; }

        public int Port { get; }

        public AddressType AddressType { get; }

        public IPEndPoint IPEndPoint => new IPEndPoint(IPAddress.Parse(AddressHelper.GetIp(Address)), Port);

        public AddressDescriptor Descriptor { get; }
        
        public override string ToString()
        {
            return string.Concat(new[] {AddressHelper.GetIp(Address), ":", Port.ToString()});
        }
        
        public override bool Equals(object obj)
        {
            var model = obj as AddressModel;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.ToString() == ToString();
        }
        
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(AddressModel model1, AddressModel model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(AddressModel model1, AddressModel model2)
        {
            return !Equals(model1, model2);
        }
    }
}