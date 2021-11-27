namespace Silky.Rpc.Runtime.Server
{
    public interface IServerHandleFallbackPolicyProvider : IHandlePolicyWithResultProvider
    {
        public event RpcHandleFallbackHandle OnHandleFallback;
    }
}