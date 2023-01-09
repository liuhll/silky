using System.Threading.Tasks;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IAsyncClientFilter : IClientFilterMetadata
{
    Task OnActionExecutionAsync(ClientInvokeExecutingContext context, ClientInvokeExecutionDelegate next);
}