using System.Threading.Tasks;
using ITestApplication.Account.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace ITestApplication.Account
{
    [ServiceRoute,AllowAnonymous]
    public interface IAccountAppService
    {
        [Author("liuhll")]
        [Governance(ExecutionTimeoutMillSeconds = 100, FailoverCount = 5)]
        Task<string> Login(LoginInput input);
    }
}