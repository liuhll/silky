using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncServerExceptionFilter : IServerFilterMetadata
{
    Task OnExceptionAsync(ServerInvokeExceptionContext context);
}