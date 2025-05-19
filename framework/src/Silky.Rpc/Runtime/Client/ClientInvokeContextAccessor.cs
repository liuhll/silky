using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client;

public class ClientInvokeContextAccessor : IClientInvokeContextAccessor, IScopedDependency
{
    internal static readonly IClientInvokeContextAccessor Null = new NullClientInvokeContextAccessor();
    private static readonly AsyncLocal<ClientInvokeContext> _storage = new ();

    private sealed class NullClientInvokeContextAccessor : IClientInvokeContextAccessor
    {
        public ClientInvokeContext? ClientInvokeContext
        {
            get => null;
            set { }
        }

        public void ClearContext()
        {
            // do nothing
        }
    }
    /// <inheritdoc/>
    [DisallowNull]
    public ClientInvokeContext? ClientInvokeContext
    {
        get { return _storage.Value; }
        set { _storage.Value = value; }
    }
    
    public void ClearContext()
    {
        _storage.Value = null;
    }
}