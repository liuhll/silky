using System.Threading;
using JetBrains.Annotations;
using Silky.Core.DependencyInjection;

namespace Silky.Core.Runtime.Rpc
{
    public class DefaultRpcContextAccessor : IRpcContextAccessor, IScopedDependency
    {
        private static AsyncLocal<RpcContextHolder> _rpcContextCurrent = new();

        [CanBeNull]
        public RpcContext RpcContext
        {
            get => _rpcContextCurrent.Value?.RpcContext;
            set
            {
                var rpcContextHolder = _rpcContextCurrent.Value;
                if (rpcContextHolder != null)
                    rpcContextHolder.RpcContext = null;
                if (value == null)
                    return;
                _rpcContextCurrent.Value = new RpcContextHolder()
                {
                    RpcContext = value
                };
            }
        }

        private class RpcContextHolder
        {
            public RpcContext RpcContext;
        }

        public void Dispose()
        {
            _rpcContextCurrent.Value = null;
        }
    }
}