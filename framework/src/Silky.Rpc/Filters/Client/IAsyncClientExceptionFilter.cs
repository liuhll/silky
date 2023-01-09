using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncClientExceptionFilter
{
    Task OnExceptionAsync(ClientInvokeExceptionContext context);
}