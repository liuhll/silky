using JetBrains.Annotations;

namespace Silky.Rpc.Address.Selector
{
    public interface IAddressSelector
    {
        IAddressModel Select([NotNull]AddressSelectContext context);

        AddressSelectorMode AddressSelectorMode { get; }

    }
}