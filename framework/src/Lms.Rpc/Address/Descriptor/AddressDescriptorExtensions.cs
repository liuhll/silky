using System;

namespace Lms.Rpc.Address.Descriptor
{
    public static class AddressDescriptorExtensions
    {
        public static IAddressModel ConvertToAddressModel(this AddressDescriptor addressDescriptor)
        {
            return new AddressModel(addressDescriptor.Address, addressDescriptor.Port,
                addressDescriptor.ServiceProtocol);
        }
    }
}