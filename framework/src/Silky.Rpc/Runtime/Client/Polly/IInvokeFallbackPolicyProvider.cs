using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokeFallbackPolicyProvider : IPolicyWithResultProvider
    {
        public event RpcInvokeFallbackHandle OnInvokeFallback;
    }
}