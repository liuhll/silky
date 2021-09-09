using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Microsoft.AspNetCore.Authorization;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Runtime.Server;

namespace GatewayDemo.AppService
{
    /// <summary>
    /// Test Proxy Service
    /// </summary>
    [ServiceRoute(template: "test/{appservice=proxy}")]
    [ServiceDescription("TestProxyAppService", Description = " Test Proxy Service")]
    public interface ITestProxyAppService
    {
        Task<TestOut> CreateProxy(TestInput testInput);

        Task<TestOut> GetProxy(string name);

        Task<string> DeleteProxy(string name, string address);

        [HttpGet]
        [AllowAnonymous]
        Task<string> MiniProfilerText();

        [HttpPatch]
        Task<string> UpdatePart(TestInput input);
    }
}