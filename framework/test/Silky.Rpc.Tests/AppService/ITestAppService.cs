using System.Threading.Tasks;
using Silky.Rpc.Tests.AppService.Dtos;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Tests.AppService
{
    [ServiceRoute("api/{appservice=test}")]
    public interface ITestAppService
    {
        [HttpPost("/test")]
        Task<long> Create(TestInput input, string test);

        [HttpPut]
        [HttpPost]
        Task<long> Update(TestInput input);

        [HttpGet]
        Task<long> Query(TestInput input);
    }
}