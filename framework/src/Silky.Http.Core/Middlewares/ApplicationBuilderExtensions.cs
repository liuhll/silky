using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Core.Serialization;
using Silky.Http.Core;
using Silky.Http.Core.Configuration;
using Silky.Http.Core.Middlewares;
using Silky.Rpc.Configuration;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSilkyWrapperResponse(this IApplicationBuilder application)
        {
            application.UseMiddleware<WrapperResponseMiddleware>();
        }

        public static void UseSilkyExceptionHandler(this IApplicationBuilder application)
        {
            application.UseExceptionHandler(handler =>
            {
                var logger = EngineContext.Current.Resolve<ILogger<SilkyException>>();
                var gatewayOptions = EngineContext.Current.GetOptionsMonitor<GatewayOptions>();
                var serializer = EngineContext.Current.Resolve<ISerializer>();
                handler.Run(context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception == null)
                        return Task.CompletedTask;
                    context.Response.ContentType = context.GetResponseContentType(gatewayOptions);
                    context.Response.SetResultStatusCode(exception.GetExceptionStatusCode());
                    context.Response.SetResultStatus(exception.GetExceptionStatus());
                    logger.LogWithMiniProfiler("Error", "Exception", exception.Message, true);
                    if (exception is IHasValidationErrors)
                    {
                        var validateErrors = exception.GetValidateErrors();
                        var responseResultData = serializer.Serialize(validateErrors);
                        return context.Response.WriteAsync(responseResultData);
                    }

                    var exceptionMessage = exception.GetExceptionMessage();
                    return context.Response.WriteAsync(exceptionMessage);
                });
            });
        }
        

        public static IApplicationBuilder UseSilkyWebSocketsProxy(this IApplicationBuilder application)
        {
            application.UseWebSockets();
            application.MapWhen(httpContext => httpContext.WebSockets.IsWebSocketRequest,
                wenSocketsApp => { wenSocketsApp.UseWebSocketsProxyMiddleware(); });
            return application;
        }

        private static IApplicationBuilder UseWebSocketsProxyMiddleware(this IApplicationBuilder application)
        {
            return application.UseMiddleware<WebSocketsProxyMiddleware>();
        }
    }
}