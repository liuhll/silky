using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Security;
using Silky.Transaction;

namespace ITestApplication.Test;

[Authorize]
[ServiceRoute]
public interface ITccTestAppService
{
    [Transaction]
    Task<string> Test(TestTccInput input);
}