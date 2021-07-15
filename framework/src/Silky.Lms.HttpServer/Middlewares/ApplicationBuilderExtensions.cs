using System.Threading.Tasks;
using Silky.Lms.Core;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Core.Extensions;
using Silky.Lms.Core.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Silky.Lms.HttpServer.Configuration;

namespace Silky.Lms.HttpServer.Middlewares
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseLmsExceptionHandler(this IApplicationBuilder application)
        {
            var gatewayOptions = EngineContext.Current.GetOptions<GatewayOptions>();
            var serializer = EngineContext.Current.Resolve<ISerializer>();

            var useDetailedExceptionPage = gatewayOptions.DisplayFullErrorStack;
            if (useDetailedExceptionPage)
            {
                application.UseDeveloperExceptionPage();
            }
            else
            {
                application.UseExceptionHandler(handler =>
                {
                    var gatewayOption = EngineContext.Current.GetOptions<GatewayOptions>();
                    if (gatewayOption.InjectMiniProfiler)
                    {
                        handler.UseMiniProfiler();
                    }

                    handler.Run(context =>
                    {
                        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                        if (exception == null)
                            return Task.CompletedTask;
                        context.Response.ContentType = "application/json;charset=utf-8";
                        EngineContext.Current.PrintToMiniProfiler("Error", "Exception", exception.Message, true);
                        if (gatewayOptions.WrapResult)
                        {
                            var responseResultDto = new ResponseResultDto()
                            {
                                Status = exception.GetExceptionStatusCode(),
                                ErrorMessage = exception.Message
                            };
                            if (exception is IHasValidationErrors)
                            {
                                responseResultDto.ValidErrors = ((IHasValidationErrors) exception).GetValidateErrors();
                            }

                            var responseResultData = serializer.Serialize(responseResultDto);
                            context.Response.ContentLength = responseResultData.GetBytes().Length;
                            context.Response.StatusCode = ResponseStatusCode.Success;
                            return context.Response.WriteAsync(responseResultData);
                        }
                        else
                        {
                            context.Response.ContentType = "text/plain";
                            context.Response.StatusCode = exception.IsBusinessException()
                                ? ResponseStatusCode.BadCode
                                : exception.IsUnauthorized()
                                    ? ResponseStatusCode.Unauthorized
                                    : ResponseStatusCode.InternalServerError;

                            if (exception is IHasValidationErrors)
                            {
                                var validateErrors = exception.GetValidateErrors();
                                var responseResultData = serializer.Serialize(validateErrors);
                                context.Response.ContentLength = responseResultData.GetBytes().Length;
                                return context.Response.WriteAsync(responseResultData);
                            }

                            context.Response.ContentLength = exception.Message.GetBytes().Length;
                            return context.Response.WriteAsync(exception.Message);
                        }
                    });
                });
            }
        }
    }
}