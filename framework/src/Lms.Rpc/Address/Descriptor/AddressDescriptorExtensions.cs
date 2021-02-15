using System;
using Lms.Core;

namespace Lms.Rpc.Address.Descriptor
{
    public static class AddressDescriptorExtensions
    {
        public static IAddressModel ConvertToAddressModel(this AddressDescriptor addressDescriptor)
        {
            if (!SingletonDictionary<string, IAddressModel>.Instance.ContainsKey(addressDescriptor.ToString()))
            {
                SingletonDictionary<string, IAddressModel>.Instance[addressDescriptor.ToString()] =
                    Singleton<IAddressModel>.Instance ?? new AddressModel(addressDescriptor.Address,
                        addressDescriptor.Port, addressDescriptor.ServiceProtocol);
            }
            SingletonDictionary<string, IAddressModel>.Instance[addressDescriptor.ToString()].InitFuseTimes();
            return SingletonDictionary<string, IAddressModel>.Instance[addressDescriptor.ToString()];
        }
    }
}