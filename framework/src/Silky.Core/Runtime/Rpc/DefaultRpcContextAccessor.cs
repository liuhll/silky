using System.Threading;
using JetBrains.Annotations;
using Silky.Core.DependencyInjection;

namespace Silky.Core.Runtime.Rpc;

public class DefaultRpcContextAccessor : IRpcContextAccessor, ISingletonDependency
{
    private static readonly AsyncLocal<RpcContextHolder> _context = new();

    public RpcContext RpcContext
    {
        get => _context.Value?.Context;
        set
        {
            if (_context.Value != null)
                _context.Value.Context = value;
            else
                _context.Value = new RpcContextHolder { Context = value };
        }
    }

    private class RpcContextHolder
    {
        public RpcContext Context;
    }
}
