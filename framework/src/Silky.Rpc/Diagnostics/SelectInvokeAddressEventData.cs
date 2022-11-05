using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Selector;

namespace Silky.Rpc.Diagnostics
{
    public class SelectInvokeAddressEventData
    {
        public ISilkyEndpoint[] EnableRpcEndpoints { get; set; }
        public ISilkyEndpoint SelectedSilkyEndpoint { get; set; }
        public ShuntStrategy ShuntStrategy { get; set; }
        public string ServiceEntryId { get; set; }
    }
}