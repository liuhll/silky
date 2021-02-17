using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Lms.Rpc.Runtime.Server;

namespace GatewayDemo.AppService
{
    public class TestProxyAppService : ITestProxyAppService
    {
        private readonly ITestAppService _testAppService;
        private readonly ICurrentServiceKey _currentServiceKey;

        public TestProxyAppService(ITestAppService testAppService,
            ICurrentServiceKey currentServiceKey)
        {
            _testAppService = testAppService;
            _currentServiceKey = currentServiceKey;
        }

        public async Task<string> CreateProxy(TestDto testDto)
        {
            _currentServiceKey.Change("v2");
            return await _testAppService.Create(testDto);
        }

        public Task<string> UpdatePart(TestDto input)
        {
            return _testAppService.UpdatePart(input);
        }
    }
}