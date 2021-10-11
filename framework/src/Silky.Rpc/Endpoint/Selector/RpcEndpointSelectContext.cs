using JetBrains.Annotations;
using Silky.Core;

namespace Silky.Rpc.Endpoint.Selector
{
    public class RpcEndpointSelectContext
    {
        public RpcEndpointSelectContext([NotNull] string monitorId, [NotNull] IRpcEndpoint[] addressModels,
            string hash = null)
        {
            Check.NotNullOrEmpty(monitorId, nameof(monitorId));
            Check.NotNull(addressModels, nameof(addressModels));
            MonitorId = monitorId;
            AddressModels = addressModels;
            Hash = hash;
        }

        [NotNull] public string MonitorId { get; }

        public string Hash { get; }

        [NotNull] public IRpcEndpoint[] AddressModels { get; }
    }
}