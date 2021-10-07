using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Silky.Core.Logging;
using Silky.Http.Core;
using Silky.Http.Core.Configuration;
using Silky.Http.Core.Middlewares;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSilkyExceptionHandler(this IApplicationBuilder application)
        {
            GatewayOptions gatewayOptions = default;
            gatewayOptions = EngineContext.Current.GetOptionsMonitor<GatewayOptions>((options, name) =>
            {
                gatewayOptions = options;
            });

            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var logger = EngineContext.Current.Resolve<ILogger<SilkyException>>();
            application.UseExceptionHandler(handler =>
            {
                if (EngineContext.Current.ContainModule("MiniProfiler"))
                {
                    handler.UseMiniProfiler();
                }

                handler.Run(context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception == null)
                        return Task.CompletedTask;
                    context.Response.ContentType = context.GetResponseContentType(gatewayOptions);
                    logger.LogWithMiniProfiler("Error", "Exception", exception.Message, true);

                    if (gatewayOptions.WrapResult)
                    {
                        var responseResultDto = new ResponseResultDto()
                        {
                            Status = exception.GetExceptionStatusCode(),
                            ErrorMessage = exception.GetExceptionMessage()
                        };
                        if (exception is IHasValidationErrors)
                        {
                            responseResultDto.ValidErrors = ((IHasValidationErrors)exception).GetValidateErrors();
                        }

                        var responseResultData = serializer.Serialize(responseResultDto);
                        context.Response.ContentLength = responseResultData.GetBytes().Length;
                        context.Response.StatusCode = ResponseStatusCode.Success;
                        context.Response.SetResultCode(exception.GetExceptionStatusCode());
                        return context.Response.WriteAsync(responseResultData);
                    }

                    context.Response.ContentType = "text/plain";
                    context.Response.SetResultCode(exception.GetExceptionStatusCode());

                    context.Response.SetExceptionResponseStatus(exception);

                    if (exception is IHasValidationErrors)
                    {
                        var validateErrors = exception.GetValidateErrors();
                        var responseResultData = serializer.Serialize(validateErrors);
                        context.Response.ContentLength = responseResultData.GetBytes().Length;
                        return context.Response.WriteAsync(responseResultData);
                    }

                    var exceptionMessage = exception.GetExceptionMessage();
                    context.Response.ContentLength = exception.Message.GetBytes().Length;
                    return context.Response.WriteAsync(exceptionMessage);
                });
            });
        }

        public static async void UseSilkyHttpServer(this IApplicationBuilder application)
        {
            var serverRegisterProvider =
                application.ApplicationServices.GetRequiredService<IServerProvider>();
            serverRegisterProvider.AddHttpServices();

            var serverRouteRegister =
                application.ApplicationServices.GetRequiredService<IServerRegister>();
            await serverRouteRegister.RegisterServer();

            var invokeMonitor =
                application.ApplicationServices.GetService<IInvokeMonitor>();
            if (invokeMonitor != null)
            {
                await invokeMonitor.ClearCache();
            }

            var serverHandleMonitor =
                application.ApplicationServices.GetService<IServerHandleMonitor>();
            if (serverHandleMonitor != null)
            {
                await serverHandleMonitor.ClearCache();
            }
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