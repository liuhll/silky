using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
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

        public static string GetClientIp(this HttpContext httpContext,bool tryUseXForwardHeader = true)
        {
            string ip = null;

            if (tryUseXForwardHeader)
            {
                ip = httpContext.Request.GetHeaderValue<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();
            }

            if (ip.IsNullOrEmpty() && httpContext?.Connection?.RemoteIpAddress != null)
            {
                ip = httpContext.Connection.RemoteIpAddress.ToString();
            }

            if (ip.IsNullOrEmpty())
            {
                ip = httpContext?.Request.GetHeaderValue<string>("REMOTE_ADDR");
            }

            return ip;
        }
        
        public static T GetHeaderValue<T>(this HttpRequest request, string headerName)
        {
            StringValues values;

            if (request.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!rawValues.IsNullOrEmpty())
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }

        private static ICollection<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .Select(s => s.Trim())
                .ToList();
        }

        public static void SetExceptionResponseStatus(this HttpResponse httpResponse, Exception exception)
        {
            var status = exception.GetExceptionStatusCode();
            httpResponse.StatusCode = status.GetHttpStatusCode().To<int>();
        }
    }
}