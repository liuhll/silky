using System.Linq;
using Silky.Core;

namespace Silky.Rpc.Address.Selector
{
    public abstract class AddressSelectorBase : IAddressSelector
    {
        public IAddressModel Select(AddressSelectContext context)
        {
            Check.NotNull(context, nameof(context));
            if (context.AddressModels.Count(p => p.Enabled) == 1 
                && AddressSelectorMode != AddressSelectorMode.HashAlgorithm)
            {
                return context.AddressModels.First(p => p.Enabled);
            }
            return SelectAddressByAlgorithm(context);
        }

        public abstract AddressSelectorMode AddressSelectorMode { get; }

        protected abstract IAddressModel SelectAddressByAlgorithm(AddressSelectContext context);
    }
}