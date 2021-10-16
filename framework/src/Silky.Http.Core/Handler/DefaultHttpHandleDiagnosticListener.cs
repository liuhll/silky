using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Http.Core.Diagnostics;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    public class DefaultHttpHandleDiagnosticListener : IHttpHandleDiagnosticListener
    {
        private static readonly DiagnosticListener s_diagnosticListener =
            new(HttpDiagnosticListenerNames.DiagnosticHttpServerListenerName);

        public long? TracingBefore(string messageId, ServiceEntry serviceEntry, HttpContext httpContext,
            object[] parameters)
        {
            if (serviceEntry.IsLocal && s_diagnosticListener.IsEnabled(HttpDiagnosticListenerNames.BeginHttpHandle))
            {
                var httpHandleEventData = new HttpHandleEventData()
                {
                    MessageId = messageId,
                    OperationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ServiceEntry = serviceEntry,
                    Parameters = parameters,
                    HttpContext = httpContext,
                };
                s_diagnosticListener.Write(HttpDiagnosticListenerNames.BeginHttpHandle, httpHandleEventData);
                return httpHandleEventData.OperationTimestamp;
            }

            return null;
        }

        public void TracingAfter(long? tracingTimestamp, string messageId, ServiceEntry serviceEntry,
            HttpContext httpContext,
            object result)
        {
            if (tracingTimestamp != null && serviceEntry.IsLocal &&
                s_diagnosticListener.IsEnabled(HttpDiagnosticListenerNames.EndHttpHandle))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var httpHandleResultData = new HttpHandleResultEventData()
                {
                    MessageId = messageId,
                    HttpContext = httpContext,
                    Result = result,
                    ServiceEntry = serviceEntry,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                };
                s_diagnosticListener.Write(HttpDiagnosticListenerNames.EndHttpHandle, httpHandleResultData);
            }
        }

        public void TracingError(long? tracingTimestamp, string messageId, ServiceEntry serviceEntry,
            HttpContext httpContext,
            Exception exception, StatusCode statusCode)
        {
            if (tracingTimestamp != null && serviceEntry.IsLocal &&
                s_diagnosticListener.IsEnabled(HttpDiagnosticListenerNames.ErrorHttpHandle))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var httpHandleResultData = new HttpHandleExceptionEventData()
                {
                    MessageId = messageId,
                    HttpContext = httpContext,
                    Exception = exception,
                    StatusCode = statusCode,
                    ServiceEntry = serviceEntry,
                    ElapsedTimeMs = now - tracingTimestamp.Value,
                };
                s_diagnosticListener.Write(HttpDiagnosticListenerNames.ErrorHttpHandle, httpHandleResultData);
            }
        }
    }
}