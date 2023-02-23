using Silky.Rpc.Routing;
using Silky.Rpc.Security;

namespace WebHostDemo.Hello;

[ServiceRoute,AllowAnonymous]
public interface IHelloAppService
{
    Task Exception();
}