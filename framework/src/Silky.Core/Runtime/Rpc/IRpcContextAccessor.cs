using System;
using JetBrains.Annotations;

namespace Silky.Core.Runtime.Rpc
{
    public interface IRpcContextAccessor : IDisposable
    {
        [CanBeNull] RpcContext RpcContext { get; set; }
    }
}