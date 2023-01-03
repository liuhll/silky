using System.Threading.Tasks;

namespace Silky.Rpc.Runtime.Server;

public interface ILocalInvoker
{
    Task InvokeAsync();

    object Result { get; }
}