using System.Threading.Tasks;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IAsyncServerExceptionFilter : IServerFilterMetadata
{
    Task OnExceptionAsync(ServerInvokeExceptionContext context);
}