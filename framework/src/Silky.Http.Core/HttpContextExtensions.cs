using System;
using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Extensions;

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
            var status = exception.GetExceptionStatusCode();
            httpResponse.StatusCode = status.GetHttpStatusCode().To<int>();
        }
    }
}