using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncServerFilter : IServerFilterMetadata
{
    Task OnActionExecutionAsync(ServerInvokeExecutingContext context, ServerExecutionDelegate next);
}