using System;
using JetBrains.Annotations;

namespace Silky.Core.Rpc
{
    public interface IRpcContextAccessor : IDisposable
    {
        [CanBeNull] RpcContext RpcContext { get; set; }
    }
}