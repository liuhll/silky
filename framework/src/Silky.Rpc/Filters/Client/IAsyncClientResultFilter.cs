using System.Threading.Tasks;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IAsyncClientResultFilter : IClientFilterMetadata
{
    Task OnResultExecutionAsync(ClientResultExecutingContext context, ClientResultExecutionDelegate next);
}