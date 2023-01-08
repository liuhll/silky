using System.Threading.Tasks;

namespace Silky.Rpc.Filters;

public interface IAsyncClientAuthorizationFilter
{
    Task OnAuthorizationAsync(ClientAuthorizationFilterContext context);
}