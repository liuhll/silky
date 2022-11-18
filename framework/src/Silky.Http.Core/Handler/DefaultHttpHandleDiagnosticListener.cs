using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Http.Core.Diagnostics;

namespace Silky.Http.Core.Handlers
{
    public class DefaultHttpHandleDiagnosticListener : IHttpHandleDiagnosticListener
    {
        private static readonly DiagnosticListener s_diagnosticListener =
            new(HttpDiagnosticListenerNames.DiagnosticHttpServerListenerName);

        public long? TracingBefore(string messageId, string serviceEntryId, bool isLocal, HttpContext httpContext,
           params object[] parameters)
        {
            if (s_diagnosticListener.IsEnabled(HttpDiagnosticListenerNames.BeginHttpHandle))
            {
                var httpHandleEventData = new HttpHandleEventData()
                {
                    MessageId = messageId,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceEntryId = serviceEntryId,
                    IsLocal = isLocal,
                    Parameters = parameters,
                    HttpContext = httpContext,
                };
                s_diagnosticListener.Write(HttpDiagnosticListenerNames.BeginHttpHandle, httpHandleEventData);
                return httpHandleEventData.OperationTimestamp;
            }

            return null;
        }

        public void TracingAfter(long? tracingTimestamp, string messageId, string serviceEntryId,bool isLocal,
            HttpContext httpContext,
            object result)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(HttpDiagnosticListenerNames.EndHttpHandle))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var httpHandleResultData = new HttpHandleResultEventData()
                {
                    MessageId = messageId,
                    HttpContext = httpContext,
                    Result = result,
                    ServiceEntryId = serviceEntryId,
                    IsLocal = isLocal,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                };
                s_diagnosticListener.Write(HttpDiagnosticListenerNames.EndHttpHandle, httpHandleResultData);
            }
        }

        public void TracingError(long? tracingTimestamp, string messageId,string serviceEntryId,bool isLocal,
            HttpContext httpContext,
            Exception exception, StatusCode statusCode)
        {
            if (tracingTimestamp != null &&
                s_diagnosticListener.IsEnabled(HttpDiagnosticListenerNames.ErrorHttpHandle))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var httpHandleResultData = new HttpHandleExceptionEventData()
                {
                    MessageId = messageId,
                    HttpContext = httpContext,
                    Exception = exception,
                    StatusCode = statusCode,
                    ServiceEntryId = serviceEntryId,
                    IsLocal = isLocal,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                };
                s_diagnosticListener.Write(HttpDiagnosticListenerNames.ErrorHttpHandle, httpHandleResultData);
            }
        }
    }
}