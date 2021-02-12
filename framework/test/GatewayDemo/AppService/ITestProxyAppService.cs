using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;

namespace GatewayDemo.AppService
{
    [ServiceRoute(template: "test/{appservice=proxy}")]
    public interface ITestProxyAppService
    {
        Task<string> CreateProxy(TestDto testDto);
    }
}