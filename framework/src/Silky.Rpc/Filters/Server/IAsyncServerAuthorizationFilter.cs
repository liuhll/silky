using System.Threading.Tasks;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IAsyncServerAuthorizationFilter : IServerFilterMetadata
{
    Task OnAuthorizationAsync(ServerAuthorizationFilterContext context);
}