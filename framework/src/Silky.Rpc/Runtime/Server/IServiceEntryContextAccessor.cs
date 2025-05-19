using System.Diagnostics.CodeAnalysis;

namespace Silky.Rpc.Runtime.Server;

public interface IServiceEntryContextAccessor
{
    [DisallowNull]
    ServiceEntryContext? ServiceEntryContext { get; set; }
    
    void ClearContext();
}