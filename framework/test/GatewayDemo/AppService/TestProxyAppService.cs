using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;

namespace GatewayDemo.AppService
{
    public class TestProxyAppService : ITestProxyAppService
    {
        private readonly ITestAppService _testAppService;

        public TestProxyAppService(ITestAppService testAppService)
        {
            _testAppService = testAppService;
        }

        public async Task<string> CreateProxy(TestDto testDto)
        {
            return await _testAppService.Create(testDto);
        }
    }
}