using JetBrains.Annotations;

namespace Silky.Rpc.Address.Selector
{
    public interface IAddressSelector
    {
        IRpcAddress Select([NotNull]AddressSelectContext context);

        AddressSelectorMode AddressSelectorMode { get; }

    }
}