using System;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public interface IClientInvokeDiagnosticListener : IScopedDependency
    {
        long? TracingBefore(RemoteInvokeMessage message, string messageId);

        void TracingAfter(long? tracingTimestamp, string messageId, string serviceEntryId,
            RemoteResultMessage remoteResultMessage);

        void TracingError(long? tracingTimestamp, string messageId, string serviceEntryId,
            StatusCode statusCode,
            Exception ex);

        void TracingSelectInvokeAddress(long? tracingTimestamp, string serviceEntryId, ShuntStrategy shuntStrategy,
            ISilkyEndpoint[] rpcEndpoints, ISilkyEndpoint selectedSilkyEndpoint);
    }
}