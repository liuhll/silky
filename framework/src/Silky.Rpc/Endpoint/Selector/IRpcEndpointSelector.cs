using JetBrains.Annotations;

namespace Silky.Rpc.Endpoint.Selector
{
    public interface IRpcEndpointSelector
    {
        IRpcEndpoint Select([NotNull] RpcEndpointSelectContext context);

        ShuntStrategy ShuntStrategy { get; }
    }
}