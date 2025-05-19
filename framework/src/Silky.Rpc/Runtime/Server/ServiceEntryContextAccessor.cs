using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server;

public class ServiceEntryContextAccessor : IServiceEntryContextAccessor, IScopedDependency
{
    internal static readonly IServiceEntryContextAccessor Null = new NullServiceEntryContextAccessor();
    private static readonly AsyncLocal<ServiceEntryContext> _storage = new AsyncLocal<ServiceEntryContext>();

    private sealed class NullServiceEntryContextAccessor : IServiceEntryContextAccessor
    {
        public ServiceEntryContext? ServiceEntryContext
        {
            get => null;
            set { }
        }

        public void ClearContext()
        {
           
        }
    }
    /// <inheritdoc/>
    [DisallowNull]
    public ServiceEntryContext? ServiceEntryContext
    {
        get { return _storage.Value; }
        set { _storage.Value = value; }
    }

    public void ClearContext()
    {
        _storage.Value = null;
    }
}