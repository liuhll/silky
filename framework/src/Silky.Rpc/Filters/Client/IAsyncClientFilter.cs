using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncClientFilter : IClientFilterMetadata
{
    Task OnActionExecutionAsync(ClientInvokeExecutingContext context, ClientInvokeExecutionDelegate next);
}