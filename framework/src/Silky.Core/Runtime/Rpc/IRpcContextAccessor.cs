using JetBrains.Annotations;

namespace Silky.Core.Runtime.Rpc
{
    public interface IRpcContextAccessor
    {
        [CanBeNull] RpcContext RpcContext { get; set; }
    }
}