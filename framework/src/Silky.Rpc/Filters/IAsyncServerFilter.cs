using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncServerFilter : IFilterMetadata
{
    Task OnActionExecutionAsync(ServerExecutingContext context, ServerExecutionDelegate next);
}