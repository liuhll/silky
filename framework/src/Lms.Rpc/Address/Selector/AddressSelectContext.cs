using System.Collections.Generic;
using JetBrains.Annotations;
using Lms.Core;

namespace Lms.Rpc.Address.Selector
{
    public class AddressSelectContext
    {
        public AddressSelectContext([NotNull] string serviceId, [NotNull] IAddressModel[] addressModels,string hashKey = null)
        {
            Check.NotNullOrEmpty(serviceId, nameof(serviceId));
            Check.NotNull(addressModels, nameof(addressModels));
            ServiceId = serviceId;
            AddressModels = addressModels;
            HashKey = hashKey;
        }

        [NotNull] public string ServiceId { get; }

        public string HashKey { get; }

        [NotNull] public IAddressModel[] AddressModels { get; }
    }
}