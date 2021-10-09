using System;
using Silky.Core.Exceptions;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public interface IClientInvokeDiagnosticListener
    {
        long? TracingBefore(RemoteInvokeMessage message, string messageId);

        void TracingAfter(long? tracingTimestamp, string messageId, string serviceEntryId,
            RemoteResultMessage remoteResultMessage);

        void TracingError(long? tracingTimestamp, string messageId, string serviceEntryId,
            StatusCode statusCode,
            Exception ex);
    }
}