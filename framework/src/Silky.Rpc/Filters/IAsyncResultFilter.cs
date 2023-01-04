using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncResultFilter
{
    Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next);
}