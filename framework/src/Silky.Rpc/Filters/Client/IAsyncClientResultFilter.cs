using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncClientResultFilter : IClientFilterMetadata
{
    Task OnResultExecutionAsync(ClientResultExecutingContext context, ClientResultExecutionDelegate next);
}