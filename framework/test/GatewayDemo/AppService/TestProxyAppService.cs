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

        public async Task<TestOut> CreateProxy(TestInput testInput)
        {
            // _currentServiceKey.Change("v2");
            return await _testAppService.Create(testInput);
        }

        public async Task<TestOut> GetProxy(string name)
        {
            return await _testAppService.Get(name);
        }

        public async Task<string> DeleteProxy(string name)
        {
            return await _testAppService.Delete(name);
        }

        public Task<string> UpdatePart(TestInput input)
        {
            return _testAppService.UpdatePart(input);
        }
    }
}