using JetBrains.Annotations;

namespace Silky.Rpc.Endpoint.Selector
{
    public interface IRpcEndpointSelector
    {
        ISilkyEndpoint Select([NotNull] RpcEndpointSelectContext context);

        ShuntStrategy ShuntStrategy { get; }
    }
}