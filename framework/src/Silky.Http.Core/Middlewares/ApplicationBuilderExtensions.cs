using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Silky.Http.Core.Configuration;
using Silky.Rpc;
using Silky.Rpc.MiniProfiler;

namespace Silky.Http.Core.Middlewares
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSilkyExceptionHandler(this IApplicationBuilder application)
        {
            var gatewayOptions = EngineContext.Current.GetOptions<GatewayOptions>();
            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var miniProfiler = EngineContext.Current.Resolve<IMiniProfiler>();

            var useDetailedExceptionPage = gatewayOptions.DisplayFullErrorStack;
            if (useDetailedExceptionPage)
            {
                application.UseDeveloperExceptionPage();
            }
            else
            {
                application.UseExceptionHandler(handler =>
                {
                    var gatewayOption = EngineContext.Current.GetOptions<SwaggerDocumentOptions>();
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
                        miniProfiler.Print("Error", "Exception", exception.Message, true);

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