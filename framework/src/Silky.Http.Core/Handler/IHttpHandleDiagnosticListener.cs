using System;
using Microsoft.AspNetCore.Http;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    public interface IHttpHandleDiagnosticListener : IScopedDependency
    {
        long? TracingBefore(string messageId, ServiceEntry serviceEntry, HttpContext httpContext, object[] parameters);

        void TracingAfter(long? tracingTimestamp, string messageId, ServiceEntry serviceEntry, HttpContext httpContext,
            object result);

        void TracingError(long? tracingTimestamp, string messageId, ServiceEntry serviceEntry, HttpContext httpContext,
            Exception exception, StatusCode statusCode);
    }
}