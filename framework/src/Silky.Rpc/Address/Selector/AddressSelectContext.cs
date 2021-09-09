using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Address.Selector
{
    public class AddressSelectContext
    {
        public AddressSelectContext([NotNull] string serviceEntryId, [NotNull] IAddressModel[] addressModels,string hash = null)
        {
            Check.NotNullOrEmpty(serviceEntryId, nameof(serviceEntryId));
            Check.NotNull(addressModels, nameof(addressModels));
            ServiceEntryId = serviceEntryId;
            AddressModels = addressModels;
            Hash = hash;
        }

        [NotNull] public string ServiceEntryId { get; }

        public string Hash { get; }

        [NotNull] public IAddressModel[] AddressModels { get; }
    }
}