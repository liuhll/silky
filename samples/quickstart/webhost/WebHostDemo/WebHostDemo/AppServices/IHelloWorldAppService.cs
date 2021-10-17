using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Routing;

namespace WebHostDemo.AppServices
{
    [ServiceRoute]
    public interface IHelloWorldAppService
    {
        [HttpGet]
        Task<string> Get();
    }
}