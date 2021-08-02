using System.Threading.Tasks;
using ITestApplication.Account.Dtos;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Rpc.Security;

namespace ITestApplication.Account
{
    [ServiceRoute,AllowAnonymous]
    public interface IAccountAppService
    {
        Task<string> Login(LoginInput input);
    }
}