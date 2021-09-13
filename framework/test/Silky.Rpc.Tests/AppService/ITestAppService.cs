using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Rpc.Tests.AppService.Dtos;
using Microsoft.AspNetCore.Mvc;

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