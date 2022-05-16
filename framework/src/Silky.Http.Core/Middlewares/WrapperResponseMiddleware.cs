using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Http.Core.Configuration;
using Silky.Rpc.Extensions;

namespace Silky.Http.Core.Middlewares
{
    public class WrapperResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GatewayOptions _gatewayOptions;
        private readonly ISerializer _serializer;

        public WrapperResponseMiddleware(RequestDelegate next, IOptionsMonitor<GatewayOptions> gatewayOptions,
            ISerializer serializer)
        {
            _next = next;
            _serializer = serializer;
            _gatewayOptions = gatewayOptions.CurrentValue;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_gatewayOptions.IgnoreWrapperPathPatterns.Any(p => Regex.IsMatch(context.Request.Path, p)) &&
                context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
            }
            else
            {
                var serviceEntry = context.GetServiceEntry();
                if (serviceEntry != null && typeof(IActionResult).IsAssignableFrom(serviceEntry.ReturnType))
                {
                    await _next.Invoke(context);
                }
                else
                {
                    var originalBodyStream = context.Response.Body;
                    await using var bodyStream = new MemoryStream();
                    try
                    {
                        context.Response.Body = bodyStream;
                        await _next.Invoke(context);
                        context.Response.Body = originalBodyStream;
                        var bodyAsText = await FormatResponse(bodyStream);
                        await HandleResponseAsync(context, bodyAsText, context.Response.StatusCode);
                    }
                    catch (Exception exception)
                    {
                        context.Response.Body = originalBodyStream;
                        await HandleExceptionAsync(context, exception);
                    }
                }
            }
        }

        private Task HandleResponseAsync(HttpContext context, string body, int code)
        {
            context.Response.ContentType = context.GetResponseContentType(_gatewayOptions);
            var status = context.Response.GetResultCode(code);
            var responseResultDto = new ResponseResultDto()
            {
                Status = (int)status,
                Code = status.ToString(),
            };
            if (status == StatusCode.Success)
            {
                var resultData = _serializer.Deserialize<dynamic>(body);
                responseResultDto.Result = resultData;
            }
            else
            {
                var exceptionFeature = context.Features.Get<ExceptionHandlerFeature>();
                if (exceptionFeature != null)
                {
                    var exceptionStatusCode = exceptionFeature.Error.GetExceptionStatusCode();
                    responseResultDto.Status = (int)exceptionStatusCode;
                    responseResultDto.Code = exceptionStatusCode.ToString();
                    responseResultDto.ErrorMessage = exceptionFeature.Error.Message;
                    if (exceptionFeature.Error is ValidationException validationException)
                    {
                        responseResultDto.ValidErrors = validationException.GetValidateErrors();
                    }
                }

                if (responseResultDto.ErrorMessage.IsNullOrEmpty())
                {
                    responseResultDto.ErrorMessage = status.GetDisplay();
                }
            }

            var jsonString = _serializer.Serialize(responseResultDto);
            return context.Response.WriteAsync(jsonString);
        }

        private async Task<string> FormatResponse(Stream bodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(bodyStream);
            var plainBodyText = await streamReader.ReadToEndAsync();
            bodyStream.Seek(0, SeekOrigin.Begin);
            return plainBodyText;
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            IEnumerable<ValidError> validErrors = null;
            var statusCode = exception.GetExceptionStatusCode();
            var status = exception.GetExceptionStatus();
            if (exception is IHasValidationErrors)
            {
                validErrors = ((IHasValidationErrors)exception).GetValidateErrors();
            }

            var responseResultDto = new ResponseResultDto()
            {
                Status = status,
                Code = statusCode.ToString(),
                ErrorMessage = exception.Message
            };
            if (validErrors != null)
            {
                responseResultDto.ValidErrors = validErrors;
            }

            context.Response.ContentType = context.GetResponseContentType(_gatewayOptions);
            var responseResultData = _serializer.Serialize(responseResultDto);
            return context.Response.WriteAsync(responseResultData);
        }
    }
}