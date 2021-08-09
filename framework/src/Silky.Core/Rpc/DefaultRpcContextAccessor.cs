using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.DependencyInjection;

namespace Silky.Core.Rpc
{
    public class DefaultRpcContextAccessor : IRpcContextAccessor, IScopedDependency
    {
        private static AsyncLocal<RpcContextHolder> _rpcContextCurrent = new();


        private IServiceScope _serviceScope;

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
                _serviceScope = EngineContext.Current.ServiceProvider.CreateScope();
                _rpcContextCurrent.Value?.RpcContext.SetServiceProvider(_serviceScope.ServiceProvider);
            }
        }

        
        public void Dispose()
        {
            _serviceScope?.Dispose();
        }

        private class RpcContextHolder
        {
            public RpcContext RpcContext;
        }
    }
}