using JetBrains.Annotations;

namespace Lms.Rpc.Address.Selector
{
    public interface IAddressSelector
    {
        IAddressModel Select([NotNull]AddressSelectContext context);
        
    }
}