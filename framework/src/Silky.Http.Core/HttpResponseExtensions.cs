using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Extensions;

namespace Silky.Http.Core;

internal static class HttpResponseExtensions
{
    public static void ConsolidateTrailers(this HttpResponse httpResponse, HttpContextServerCallContext context)
    {
        var gatewayOptions = EngineContext.Current.GetOptionsMonitor<GatewayOptions>();
        httpResponse.ContentType = context.HttpContext.GetResponseContentType(gatewayOptions);
        httpResponse.StatusCode = ResponseStatusCode.Success;
        httpResponse.SetResultStatusCode(StatusCode.Success);
        httpResponse.SetResultStatus((int)StatusCode.Success);
    }

    public static void ConsolidateTrailers(this HttpResponse httpResponse, HttpContextServerCallContext context,
        Exception exception)
    {
        var gatewayOptions = EngineContext.Current.GetOptionsMonitor<GatewayOptions>();
        httpResponse.ContentType = context.HttpContext.GetResponseContentType(gatewayOptions);
        context.HttpContext.Features.Set(new ExceptionHandlerFeature()
        {
            Error = exception,
            Path = context.HttpContext.Request.Path
        });
        httpResponse.SetExceptionResponseStatus(exception);
        httpResponse.SetResultStatusCode(exception.GetExceptionStatusCode());
        httpResponse.SetResultStatus(exception.GetExceptionStatus());
    }
}