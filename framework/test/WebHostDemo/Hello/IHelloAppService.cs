using Silky.Rpc.Routing;
using Silky.Rpc.Security;
using Silky.Transaction;

namespace WebHostDemo.Hello;

[ServiceRoute,AllowAnonymous]
public interface IHelloAppService
{
    Task Exception();

    [Transaction]
    Task TccTest();
}