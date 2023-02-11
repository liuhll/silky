using System;
using Microsoft.AspNetCore.Http;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;

namespace Silky.Http.Core.Handlers
{
    public interface IHttpHandleDiagnosticListener : ISingletonDependency
    {
        long? TracingBefore(string messageId, string serviceEntryId, bool isLocal, HttpContext httpContext,
            params object[] parameters);

        void TracingAfter(long? tracingTimestamp, string messageId, string serviceEntryId, bool isLocal,
            HttpContext httpContext,
            object result);

        void TracingError(long? tracingTimestamp, string messageId, string serviceEntryId, bool isLocal,
            HttpContext httpContext,
            Exception exception, StatusCode statusCode);
    }
}