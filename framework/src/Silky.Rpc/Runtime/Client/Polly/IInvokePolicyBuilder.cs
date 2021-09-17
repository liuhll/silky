using Polly;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokePolicyBuilder
    {
        IAsyncPolicy<object> Build(ServiceEntry serviceEntry, object[] parameters);
    }
}