using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Filters;

namespace ITestApplication.Filters
{
    public class TestClientFilterAttribute : ClientFilterAttribute
    {
        public ILogger<TestClientFilterAttribute> Logger { get; set; }

        public TestClientFilterAttribute(int order)
        {
            Logger = NullLogger<TestClientFilterAttribute>.Instance;
        }

        public override void OnActionExecuting(ClientInvokeExecutingContext context)
        {
            Logger.LogInformation("test client filter");
        }
    }
}