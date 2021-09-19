using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Microsoft.AspNetCore.Http;
using Silky.Rpc.Runtime.Server;
using StackExchange.Profiling;

namespace GatewayDemo.AppService
{
    public class TestProxyAppService : ITestProxyAppService
    {
        private readonly ITestAppService _testAppService;
        private readonly IServiceKeyExecutor _serviceKeyExecutor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestProxyAppService(ITestAppService testAppService,
            IServiceKeyExecutor serviceKeyExecutor,
            IHttpContextAccessor httpContextAccessor)
        {
            _testAppService = testAppService;
            _serviceKeyExecutor = serviceKeyExecutor;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TestOut> CreateProxy(TestInput testInput)
        {
            // _serviceKeyExecutor.ChangeServiceKey("v2");
            var result = await _testAppService.Create(testInput);
            return result;
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

        public async Task<string> DeleteProxy(string name, string address)
        {
            return await _testAppService.Delete(new TestInput() { Name = name, Address = address });
        }

        public async Task<string> UpdatePart(TestInput input)
        {
            return await _serviceKeyExecutor.Execute(() => _testAppService.UpdatePart(input), "v2");
        }
    }
}