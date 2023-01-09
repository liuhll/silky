using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncServerAuthorizationFilter : IServerFilterMetadata
{
    Task OnAuthorizationAsync(ServerAuthorizationFilterContext context);
}