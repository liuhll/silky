using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokeFailoverPolicyProvider : IPolicyWithResultProvider
    {
        event RpcInvokeFailoverHandle OnInvokeFailover;

        FailoverType FailoverType { get; }
    }
}