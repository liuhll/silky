using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Http.Core;
using Silky.Http.Core.Configuration;
using Silky.Http.Identity.Authorization.Requirements;

namespace Silky.Http.Identity.Authorization
{
    public class SilkyAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly IAuthorizationMiddlewareResultHandler _handler;
        private GatewayOptions _gatewayOptions;
        private readonly ISerializer _serializer;

        public SilkyAuthorizationMiddlewareResultHandler(IOptionsMonitor<GatewayOptions> gatewayOptions,
            ISerializer serializer)
        {
            _serializer = serializer;
            _handler = new AuthorizationMiddlewareResultHandler();
            _gatewayOptions = gatewayOptions.CurrentValue;
            gatewayOptions.OnChange((options, s) => _gatewayOptions = options);
        }

        public async Task HandleAsync(
            RequestDelegate requestDelegate,
            HttpContext httpContext,
            AuthorizationPolicy authorizationPolicy,
            PolicyAuthorizationResult policyAuthorizationResult)
        {
            Task WriteResponse(string message)
            {
                if (_gatewayOptions.WrapResult)
                {
                    httpContext.Response.ContentType = "application/json;charset=utf-8";
                    var responseResultDto = new ResponseResultDto()
                    {
                        Status = StatusCode.UnAuthentication,
                        ErrorMessage = message
                    };


                    var responseResultData = _serializer.Serialize(responseResultDto);
                    httpContext.Response.ContentLength = responseResultData.GetBytes().Length;
                    httpContext.Response.StatusCode = ResponseStatusCode.Success;
                    return httpContext.Response.WriteAsync(responseResultData);
                }
                else
                {
                    httpContext.Response.ContentType = "text/plain";
                    httpContext.Response.ContentLength = message.GetBytes().Length;
                    httpContext.Response.StatusCode = ResponseStatusCode.Unauthorized;
                    return httpContext.Response.WriteAsync(message);
                }
            }

            if (policyAuthorizationResult.AuthorizationFailure != null)
            {
                await WriteResponse(
                    $"You do not have permission to access {httpContext.Request.Path}-{httpContext.Request.Method} webapi");
            }
            else
            {
                await _handler.HandleAsync(requestDelegate, httpContext, authorizationPolicy,
                    policyAuthorizationResult);
            }
        }
    }
}