using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Routing;
using Silky.Rpc.Security;

namespace WebHostDemo.AppServices
{
    [ServiceRoute]
    public interface IHelloWorldAppService
    {
        [HttpGet]
        [AllowAnonymous]
        Task<string> Get();
    }
}