using System;
using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Http.Core.Configuration;

namespace Silky.Http.Core
{
    public static class HttpContextExtensions
    {
        public static string GetResponseContentType(this HttpContext httpContext, GatewayOptions gatewayOptions)
        {
            var defaultResponseContextType = "application/json;charset=utf-8";
            if (httpContext.Request.Headers.ContainsKey("Accept"))
            {
                if (httpContext.Request.Headers["Accept"] != "*/*")
                {
                    return httpContext.Request.Headers["Accept"];
                }
            }

            if (!gatewayOptions.ResponseContentType.IsNullOrEmpty())
            {
                return gatewayOptions.ResponseContentType;
            }

            return defaultResponseContextType;
        }
        
        public static void SetExceptionResponseStatus(this HttpResponse httpResponse, Exception exception)
        {
            if (exception.IsBusinessException() || exception.IsUserFriendlyException())
            {
                httpResponse.StatusCode = ResponseStatusCode.BadCode;
            }

            if (exception.IsUnauthorized())
            {
                httpResponse.StatusCode = ResponseStatusCode.Unauthorized;
            }

            httpResponse.StatusCode = ResponseStatusCode.InternalServerError;
        }
    }
}