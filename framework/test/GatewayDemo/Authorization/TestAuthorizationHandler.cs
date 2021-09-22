using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Silky.Http.Core;
using Silky.Http.Identity.Authorization.Handlers;

namespace GatewayDemo.Authorization
{
    public class TestAuthorizationHandler : SilkyAuthorizationHandler
    {
        private readonly ILogger<TestAuthorizationHandler> _logger;

        public TestAuthorizationHandler(ILogger<TestAuthorizationHandler> logger)
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