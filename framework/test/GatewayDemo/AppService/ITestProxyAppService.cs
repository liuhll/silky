using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc;

namespace GatewayDemo.AppService
{
    [ServiceRoute(template: "test/{appservice=proxy}")]
    public interface ITestProxyAppService
    {
        Task<TestOut> CreateProxy(TestInput testInput);

        Task<TestOut> GetProxy(string name);

        Task<string> DeleteProxy(string name);

        [HttpGet]
        Task<string> MiniProfilerText();

        [HttpPatch]
        Task<string> UpdatePart(TestInput input);
    }
}