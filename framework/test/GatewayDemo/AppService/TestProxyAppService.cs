using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Microsoft.AspNetCore.Http;
using Silky.Lms.Rpc.Runtime.Server;
using StackExchange.Profiling;

namespace GatewayDemo.AppService
{
    public class TestProxyAppService : ITestProxyAppService
    {
        private readonly ITestAppService _testAppService;
        private readonly ICurrentServiceKey _currentServiceKey;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TestProxyAppService(ITestAppService testAppService,
            ICurrentServiceKey currentServiceKey, 
            IHttpContextAccessor httpContextAccessor)
        {
            _testAppService = testAppService;
            _currentServiceKey = currentServiceKey;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TestOut> CreateProxy(TestInput testInput)
        {
            // _currentServiceKey.Change("v2");
            return await _testAppService.Create(testInput);
        }

        public async Task<string> MiniProfilerText()
        {
            var html = MiniProfiler.Current.RenderIncludes(_httpContextAccessor.HttpContext).ToString();
            return html;

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