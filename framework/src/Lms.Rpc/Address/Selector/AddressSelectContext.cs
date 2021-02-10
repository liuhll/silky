using System.Collections.Generic;
using JetBrains.Annotations;
using Lms.Core;

namespace Lms.Rpc.Address.Selector
{
    public class AddressSelectContext
    {
        public AddressSelectContext([NotNull] string serviceId, [NotNull] IAddressModel[] addressModels,string hash = null)
        {
            Check.NotNullOrEmpty(serviceId, nameof(serviceId));
            Check.NotNull(addressModels, nameof(addressModels));
            ServiceId = serviceId;
            AddressModels = addressModels;
            Hash = hash;
        }

        [NotNull] public string ServiceId { get; }

        public string Hash { get; }

        [NotNull] public IAddressModel[] AddressModels { get; }
    }
}