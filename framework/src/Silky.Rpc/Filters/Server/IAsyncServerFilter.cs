using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncServerFilter : IServerFilterMetadata
{
    Task OnActionExecutionAsync(ServerExecutingContext context, ServerExecutionDelegate next);
}