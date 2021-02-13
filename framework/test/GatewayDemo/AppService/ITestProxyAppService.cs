using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc;

namespace GatewayDemo.AppService
{
    [ServiceRoute(template: "test/{appservice=proxy}")]
    public interface ITestProxyAppService
    {
        Task<string> CreateProxy(TestDto testDto);
        
        [HttpPatch]
        Task<string> UpdatePart(TestDto input);
    }
}