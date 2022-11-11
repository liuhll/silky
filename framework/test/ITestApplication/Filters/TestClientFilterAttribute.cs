using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace ITestApplication.Filters
{
    public class TestClientFilterAttribute : ClientFilterAttribute
    {
        public ILogger<TestClientFilterAttribute> Logger { get; set; }

        public TestClientFilterAttribute(int order) : base(order)
        {
            Logger = NullLogger<TestClientFilterAttribute>.Instance;
        }

        public override void OnActionExecuting(RemoteInvokeMessage remoteInvokeMessage)
        {
            Logger.LogInformation("test client filter");
        }
    }
}