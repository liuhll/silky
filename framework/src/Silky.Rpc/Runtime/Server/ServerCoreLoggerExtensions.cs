using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Silky.Rpc.Runtime.Server;

public static class ServerCoreLoggerExtensions
{
    public static IDisposable? ActionScope(this ILogger logger, ServiceEntry serviceEntry)
    {
        return logger.BeginScope(new ServiceEntryLogScope(serviceEntry));
    }
    
    private sealed class ServiceEntryLogScope : IReadOnlyList<KeyValuePair<string, object>>
    {
        private readonly ServiceEntry _serviceEntry;

        public ServiceEntryLogScope(ServiceEntry serviceEntry)
        {
            if (serviceEntry == null)
            {
                throw new ArgumentNullException(nameof(serviceEntry));
            }

            _serviceEntry = serviceEntry;
        }

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return new KeyValuePair<string, object>("ServiceEntryId", _serviceEntry.Id);
                }
                else if (index == 1)
                {
                    return new KeyValuePair<string, object>("ServiceEntryName", _serviceEntry.MethodInfo.Name ?? string.Empty);
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
            return _serviceEntry.Id ?? string.Empty;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}