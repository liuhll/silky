using Polly;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokePolicyBuilder
    {
        IAsyncPolicy<object> Build(string serviceEntryId, object[] parameters);
    }
}