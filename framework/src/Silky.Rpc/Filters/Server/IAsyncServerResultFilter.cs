using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncServerResultFilter
{
    Task OnResultExecutionAsync(ServerResultExecutingContext context, ResultExecutionDelegate next);
}