using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Silky.Http.Identity.Authorization.Handlers;

namespace GatewayDemo.Authorization
{
    public class TestAuthorizationHandlerBase : SilkyAuthorizationHandlerBase
    {
        private readonly ILogger<TestAuthorizationHandlerBase> _logger;

        public TestAuthorizationHandlerBase(ILogger<TestAuthorizationHandlerBase> logger)
        {
            _logger = logger;
        }

        public async override Task<bool> PipelineAsync(AuthorizationHandlerContext context,
            DefaultHttpContext httpContext)
        {
            // var serviceEntry = httpContext.GetServiceEntry();
            // if (serviceEntry.Services.Id.Contains("ITestApplication"))
            // {
            //     _logger.LogInformation($"{serviceEntry.Id} has permission");
            //     return true;
            // }

            return true;
        }
    }
}