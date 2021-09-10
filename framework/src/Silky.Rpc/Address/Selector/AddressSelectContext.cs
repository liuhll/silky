using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Address.Selector
{
    public class AddressSelectContext
    {
        public AddressSelectContext([NotNull] string monitorId, [NotNull] IAddressModel[] addressModels,string hash = null)
        {
            Check.NotNullOrEmpty(monitorId, nameof(monitorId));
            Check.NotNull(addressModels, nameof(addressModels));
            MonitorId = monitorId;
            AddressModels = addressModels;
            Hash = hash;
        }

        [NotNull] public string MonitorId { get; }

        public string Hash { get; }

        [NotNull] public IAddressModel[] AddressModels { get; }
    }
}