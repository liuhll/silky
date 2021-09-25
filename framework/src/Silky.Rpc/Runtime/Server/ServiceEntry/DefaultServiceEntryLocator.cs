using System.Linq;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceEntryLocator : IServiceEntryLocator
    {
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly ServiceEntryCache _serviceEntryCache;

        public DefaultServiceEntryLocator(IServiceEntryManager serviceEntryManager,
            ServiceEntryCache serviceEntryCache)
        {
            _serviceEntryManager = serviceEntryManager;
            _serviceEntryCache = serviceEntryCache;
        }

        public ServiceEntry GetServiceEntryById(string id)
        {
            if (_serviceEntryCache.TryGetServiceEntry(id, out ServiceEntry serviceEntry))
            {
                return serviceEntry;
            }

            serviceEntry = _serviceEntryManager.GetAllEntries()
                .FirstOrDefault(p => p.ServiceEntryDescriptor.Id == id);
            if (serviceEntry != null)
            {
                _serviceEntryCache.UpdateServiceEntryCache(serviceEntry);
            }

            return serviceEntry;
        }

        public ServiceEntry GetLocalServiceEntryById(string id)
        {
            if (_serviceEntryCache.TryGetLocalServiceEntry(id, out ServiceEntry serviceEntry))
            {
                return serviceEntry;
            }

            serviceEntry = _serviceEntryManager.GetLocalEntries()
                .FirstOrDefault(p => p.ServiceEntryDescriptor.Id == id);
            if (serviceEntry != null)
            {
                _serviceEntryCache.UpdateLocalServiceEntryCache(serviceEntry);
            }

            return serviceEntry;
        }
    }
}