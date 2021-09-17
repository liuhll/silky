using System.Threading.Tasks;
using Polly;
using Silky.Rpc.Address;

namespace Silky.Rpc.Runtime.Client
{
    public delegate Task RpcInvokeFallbackHandle(DelegateResult<object> delegateResult, Context context);

    public delegate Task RpcInvokeFailoverHandle(DelegateResult<object> delegateResult, int retryNumber,
        Context context, IAddressModel serviceAddressModel);
}