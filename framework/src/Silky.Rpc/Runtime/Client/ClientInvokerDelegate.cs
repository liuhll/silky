using System.Threading.Tasks;
using Polly;
using Silky.Rpc.Address;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Runtime.Client
{
    public delegate Task RpcInvokeFallbackHandle(DelegateResult<object> delegateResult, Context context);

    public delegate Task RpcInvokeFailoverHandle(DelegateResult<object> delegateResult, int retryNumber,
        Context context, IRpcEndpoint serviceRpcEndpoint, FailoverType failoverType);
}