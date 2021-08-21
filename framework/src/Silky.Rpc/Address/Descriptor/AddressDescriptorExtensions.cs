using System;
using Silky.Core;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Address.Descriptor
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

        public static string ConvertToAddress(this AddressDescriptor addressDescriptor)
        {
            string address = String.Empty;
            switch (addressDescriptor.ServiceProtocol)
            {
                case ServiceProtocol.Http:
                case ServiceProtocol.Https:
                case ServiceProtocol.Ws:
                case ServiceProtocol.Wss:
                case ServiceProtocol.Mqtt:
                    address =
                        $"{addressDescriptor.ServiceProtocol.ToString().ToLower()}://{addressDescriptor.Address}:{addressDescriptor.Port}";
                    break;
                case ServiceProtocol.Tcp:
                    address =
                        $"{addressDescriptor.Address}:{addressDescriptor.Port}";
                    break;
            }

            return address;
        }
    }
}