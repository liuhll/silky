using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client;

public static class ClientCoreLoggerExtensions
{
    public static IDisposable? ActionScope(this ILogger logger, RemoteInvokeMessage remoteInvokeMessage)
    {
        return logger.BeginScope(new RemoteInvokeMessageLogScope(remoteInvokeMessage));
    }

    private sealed class RemoteInvokeMessageLogScope : IReadOnlyList<KeyValuePair<string, object>>
    {

        private readonly RemoteInvokeMessage _remoteInvokeMessage;
        public RemoteInvokeMessageLogScope(RemoteInvokeMessage remoteInvokeMessage)
        {
            _remoteInvokeMessage = remoteInvokeMessage;
        }
        
        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return new KeyValuePair<string, object>("ServiceEntryId", _remoteInvokeMessage.ServiceEntryId);
                }
                else if (index == 1)
                {
                    return new KeyValuePair<string, object>("ServiceId", _remoteInvokeMessage.ServiceId ?? string.Empty);
                }
                throw new IndexOutOfRangeException(nameof(index));
            }
        }
        
        public int Count => 2;
        
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }
        
        public override string ToString()
        {
            // We don't include the _action.Id here because it's just an opaque guid, and if
            // you have text logging, you can already use the requestId for correlation.
            return _remoteInvokeMessage.ServiceEntryId ?? string.Empty;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}