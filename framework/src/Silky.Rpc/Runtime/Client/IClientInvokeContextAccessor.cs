using System.Diagnostics.CodeAnalysis;

namespace Silky.Rpc.Runtime.Client;

public interface IClientInvokeContextAccessor
{
    [DisallowNull]
    ClientInvokeContext? ClientInvokeContext { get; set; }
}