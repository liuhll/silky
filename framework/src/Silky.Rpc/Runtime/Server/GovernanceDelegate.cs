using System.Threading.Tasks;
using Polly;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public delegate Task RpcInvokeFallbackHandle(DelegateResult<object> delegateResult, Context context);

    public delegate Task RpcHandleFallbackHandle(DelegateResult<RemoteResultMessage> delegateResult, Context context);

    public delegate Task RpcInvokeFailoverHandle(DelegateResult<object> delegateResult, int retryNumber,
        Context context, ISilkyEndpoint serviceSilkyEndpoint, FailoverType failoverType);
}