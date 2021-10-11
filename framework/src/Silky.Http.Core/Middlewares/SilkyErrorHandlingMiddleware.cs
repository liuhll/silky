using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.Serialization;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Extensions;

namespace Silky.Http.Core.Middlewares
{
    public class SilkyErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GatewayOptions _gatewayOptions;
        private readonly ISerializer _serializer;
        private readonly ILogger<SilkyErrorHandlingMiddleware> _logger;

        public SilkyErrorHandlingMiddleware(RequestDelegate next, IOptionsMonitor<GatewayOptions> gatewayOptions,
            ISerializer serializer, ILogger<SilkyErrorHandlingMiddleware> logger)
        {
            _next = next;
            _serializer = serializer;
            _logger = logger;
            _gatewayOptions = gatewayOptions.CurrentValue;
        }

        public async Task Invoke(HttpContext context)
        {
            Exception exception = null;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogWithMiniProfiler("Error", "Exception", ex.Message, true);
                exception = ex;
            }
            finally
            {
                if (exception == null)
                {
                    var httpStatusCode = context.Response.StatusCode;
                    var statusCode = StatusCode.Success;
                    var msg = "";
                    if (httpStatusCode == 401)
                    {
                        msg = "You have not logged in to the system.";
                        statusCode = StatusCode.UnAuthentication;
                    }
                    else if (httpStatusCode == 404)
                    {
                        msg = "Service not found";
                        statusCode = StatusCode.NotFindServiceRoute;
                    }
                    else if (httpStatusCode == 502)
                    {
                        msg = "Request error";
                        statusCode = StatusCode.RequestError;
                    }
                    else if (httpStatusCode != 200)
                    {
                        msg = "Unknown Mistake";
                        statusCode = StatusCode.ServerError;
                    }

                    if (!msg.IsNullOrWhiteSpace())
                    {
                        exception = new SilkyException(msg, statusCode);
                    }
                }

                if (exception != null)
                {
                    await HandleExceptionAsync(context, exception);
                }
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.SetResultCode(exception.GetExceptionStatusCode());
            context.Response.SetExceptionResponseStatus(exception);
            context.Response.ContentType = context.GetResponseContentType(_gatewayOptions);

            IEnumerable<ValidError> validErrors = null;
            var statusCode = exception.GetExceptionStatusCode();
            if (exception is IHasValidationErrors)
            {
                validErrors = ((IHasValidationErrors)exception).GetValidateErrors();
            }

            if (_gatewayOptions.WrapResult)
            {
                var responseResultDto = new ResponseResultDto()
                {
                    Status = exception.GetExceptionStatusCode(),
                    ErrorMessage = exception.Message
                };
                if (validErrors != null)
                {
                    responseResultDto.ValidErrors = validErrors;
                }

                var responseResultData = _serializer.Serialize(responseResultDto);
                context.Response.ContentLength = responseResultData.GetBytes().Length;
                context.Response.StatusCode = ResponseStatusCode.Success;
                context.Response.SetResultCode(statusCode);
                await context.Response.WriteAsync(responseResultData);
            }
            else
            {
                context.Response.SetResultCode(statusCode);

                if (validErrors != null)
                {
                    var responseResultData = _serializer.Serialize(validErrors);
                    context.Response.ContentLength = responseResultData.GetBytes().Length;
                    await context.Response.WriteAsync(responseResultData);
                }
                else
                {
                    context.Response.ContentLength = exception.Message.GetBytes().Length;
                    await context.Response.WriteAsync(exception.Message);
                }
            }
        }
    }
}