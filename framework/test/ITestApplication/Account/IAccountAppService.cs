using System.Threading.Tasks;
using ITestApplication.Account.Dtos;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace ITestApplication.Account
{
    [ServiceRoute, AllowAnonymous]
    public interface IAccountAppService
    {
        [Author("liuhll")]
        // [Governance(TimeoutMillSeconds = 5, RetryTimes = 2)]
        Task<string> Login(LoginInput input);

        [HttpPost, UnWrapperResult]
        Task<int> CheckUrl();

        [HttpPost, UnWrapperResult]
        Task<int> SubmitUrl([FromForm]SpecificationWithTenantAuth authInfo);
    }
}