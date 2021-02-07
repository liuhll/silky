using System.Collections.Concurrent;
using Lms.Core.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Runtime.Server
{
    public class ServiceEntryCache : ISingletonDependency
    {
        private ConcurrentDictionary<string, ServiceEntry> _allServiceEntriesCache =
            new ConcurrentDictionary<string, ServiceEntry>();

        private ConcurrentDictionary<string, ServiceEntry> _localServiceEntriesCache =
            new ConcurrentDictionary<string, ServiceEntry>();

        private ConcurrentDictionary<(string, HttpMethod), ServiceEntry> _requestServiceEntriesCache =
            new ConcurrentDictionary<(string, HttpMethod), ServiceEntry>();

        public bool TryGetLocalServiceEntry(string serviceId, out ServiceEntry serviceEntry)
        {
            return _localServiceEntriesCache.TryGetValue(serviceId, out serviceEntry);
        }

        public bool TryGetServiceEntry(string serviceId, out ServiceEntry serviceEntry)
        {
            return _allServiceEntriesCache.TryGetValue(serviceId, out serviceEntry);
        }

        public bool TryGetRequestServiceEntry((string, HttpMethod) requestApi, out ServiceEntry serviceEntry)
        {
            return _requestServiceEntriesCache.TryGetValue(requestApi, out serviceEntry);
        }

        public void UpdateLocalServiceEntryCache(string serviceId, ServiceEntry serviceEntry)
        {
            _localServiceEntriesCache.AddOrUpdate(serviceId, serviceEntry, (k, _) => serviceEntry);
        }
        
        public void UpdateServiceEntryCache(string serviceId, ServiceEntry serviceEntry)
        {
            _allServiceEntriesCache.AddOrUpdate(serviceId, serviceEntry, (k, _) => serviceEntry);
        }
        
        public void UpdateRequestServiceEntryCache((string, HttpMethod) requestApi, ServiceEntry serviceEntry)
        {
            _requestServiceEntriesCache.AddOrUpdate(requestApi, serviceEntry, (key, _) => serviceEntry);
        }
    }
}