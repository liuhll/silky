using System.Linq;
using Silky.Core;

namespace Silky.Rpc.Endpoint.Selector
{
    public abstract class RpcEndpointSelectorBase : IRpcEndpointSelector
    {
        public IRpcEndpoint Select(RpcEndpointSelectContext context)
        {
            Check.NotNull(context, nameof(context));
            if (context.AddressModels.Count(p => p.Enabled) == 1
                && ShuntStrategy != ShuntStrategy.HashAlgorithm)
            {
                return context.AddressModels.First(p => p.Enabled);
            }

            return SelectAddressByAlgorithm(context);
        }

        public abstract ShuntStrategy ShuntStrategy { get; }

        protected abstract IRpcEndpoint SelectAddressByAlgorithm(RpcEndpointSelectContext context);
    }
}