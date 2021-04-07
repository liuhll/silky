using JetBrains.Annotations;

namespace Silky.Lms.Rpc.Address.Selector
{
    public interface IAddressSelector
    {
        IAddressModel Select([NotNull]AddressSelectContext context);

        AddressSelectorMode AddressSelectorMode { get; }

    }
}