using Polly;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokeFallbackPolicyProvider : ISingletonDependency
    {

        IAsyncPolicy<object> Create(string serviceEntryId, object[] parameters);
        
        public event RpcInvokeFallbackHandle OnInvokeFallback;
    }
}