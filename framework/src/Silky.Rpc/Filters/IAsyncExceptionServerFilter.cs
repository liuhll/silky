using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncExceptionServerFilter : IServerFilterMetadata
{
    Task OnExceptionAsync(ServerExceptionContext context);
}