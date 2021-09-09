using System.Collections.Concurrent;
using System.Linq;
using Silky.Core.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntryCache : ISingletonDependency
    {
        private ConcurrentDictionary<string, ServiceEntry> _allServiceEntriesCache = new();

        private ConcurrentDictionary<string, ServiceEntry> _localServiceEntriesCache = new();

        private ConcurrentDictionary<(string, HttpMethod), ServiceEntry> _requestServiceEntriesCache = new();

        private readonly IServiceEntryManager _serviceEntryManager;

        public ServiceEntryCache(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
            _serviceEntryManager.OnUpdate += (sender, entry) => { UpdateServiceEntryCache(entry); };
        }

        public bool TryGetLocalServiceEntry(string serviceEntryId, out ServiceEntry serviceEntry)
        {
            return _localServiceEntriesCache.TryGetValue(serviceEntryId, out serviceEntry);
        }

        public bool TryGetServiceEntry(string serviceEntryId, out ServiceEntry serviceEntry)
        {
            return _allServiceEntriesCache.TryGetValue(serviceEntryId, out serviceEntry);
        }

        public bool TryGetRequestServiceEntry((string, HttpMethod) requestApi, out ServiceEntry serviceEntry)
        {
            return _requestServiceEntriesCache.TryGetValue(requestApi, out serviceEntry);
        }

        public void UpdateLocalServiceEntryCache(ServiceEntry serviceEntry)
        {
            _localServiceEntriesCache.AddOrUpdate(serviceEntry.ServiceEntryDescriptor.Id, serviceEntry,
                (k, _) => serviceEntry);
        }

        public void UpdateServiceEntryCache(ServiceEntry serviceEntry)
        {
            _allServiceEntriesCache.AddOrUpdate(serviceEntry.ServiceEntryDescriptor.Id, serviceEntry,
                (k, _) => serviceEntry);
            if (serviceEntry.IsLocal)
            {
                UpdateLocalServiceEntryCache(serviceEntry);
            }

            var requestServiceCache = _requestServiceEntriesCache.FirstOrDefault(p =>
                p.Value.ServiceEntryDescriptor.Id.Equals(serviceEntry.ServiceEntryDescriptor.Id));
            _requestServiceEntriesCache.TryRemove(requestServiceCache.Key, out var value);
        }

        public void UpdateRequestServiceEntryCache((string, HttpMethod) requestApi, ServiceEntry serviceEntry)
        {
            _requestServiceEntriesCache.AddOrUpdate(requestApi, serviceEntry, (key, _) => serviceEntry);
        }
    }
}