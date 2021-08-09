using Silky.Core.DependencyInjection;

namespace Silky.Core.Rpc
{
    public class DefaultRpcContextAccessor : IRpcContextAccessor, IScopedDependency
    {
        public RpcContext RpcContext => RpcContext.GetContext();
    }
}