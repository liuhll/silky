using Polly;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IPolicyBuilder
    {
        IAsyncPolicy<object> Build(ServiceEntry serviceEntry, object[] parameters);

        event RpcInvokeFallbackHandle OnInvokeFallback;
    }
}