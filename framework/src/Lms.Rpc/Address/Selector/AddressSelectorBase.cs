using System.Linq;
using Lms.Core;

namespace Lms.Rpc.Address.Selector
{
    public abstract class AddressSelectorBase : IAddressSelector
    {
        public IAddressModel Select(AddressSelectContext context)
        {
            Check.NotNull(context, nameof(context));
            if (context.AddressModels.Count(p => p.Enabled) == 1)
            {
                return context.AddressModels.First(p => p.Enabled);
            }

            return SelectAddressByAlgorithm(context);
        }

        protected abstract IAddressModel SelectAddressByAlgorithm(AddressSelectContext context);
    }
}