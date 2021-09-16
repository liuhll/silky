using System.Threading.Tasks;
using Polly;

namespace Silky.Rpc.Runtime.Client
{
    public delegate Task RpcInvokeFallbackHandle(DelegateResult<object> delegateResult, Context context);
}