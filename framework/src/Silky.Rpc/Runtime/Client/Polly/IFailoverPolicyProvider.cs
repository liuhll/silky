namespace Silky.Rpc.Runtime.Client
{
    public interface IFailoverPolicyProvider : IPolicyWithResultProvider
    {
        event RpcInvokeFailoverHandle OnInvokeFailover;

        FailoverType FailoverType { get; }
    }
}