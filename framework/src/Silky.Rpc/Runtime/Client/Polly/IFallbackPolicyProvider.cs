using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IFallbackPolicyProvider : IPolicyWithResultProvider
    {
        public event RpcInvokeFallbackHandle OnInvokeFallback;
    }
}