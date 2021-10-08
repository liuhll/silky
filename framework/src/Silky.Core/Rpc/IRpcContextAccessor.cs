using JetBrains.Annotations;

namespace Silky.Core.Rpc
{
    public interface IRpcContextAccessor
    {
        [CanBeNull] RpcContext RpcContext { get; set; }
    }
}