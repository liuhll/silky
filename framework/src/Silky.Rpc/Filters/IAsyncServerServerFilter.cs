using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncServerServerFilter : IServerFilterMetadata
{
    Task OnActionExecutionAsync(ServerExecutingContext context, ServerExecutionDelegate next);
}