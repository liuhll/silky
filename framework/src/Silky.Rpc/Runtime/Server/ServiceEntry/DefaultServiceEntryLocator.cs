using System.Linq;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

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

        public ServiceEntry GetServiceEntryByApi(string path, HttpMethod httpMethod)
        {
            if (_serviceEntryCache.TryGetRequestServiceEntry((path, httpMethod), out ServiceEntry serviceEntry))
            {
                return serviceEntry;
            }

            serviceEntry = _serviceEntryManager.GetAllEntries()
                .OrderByDescending(p => p.Router.RouteTemplate.Order)
                .FirstOrDefault(p => p.Router.IsMatch(path, httpMethod));
            if (serviceEntry != null && serviceEntry.ParameterDescriptors.All(p => p.From != ParameterFrom.Path))
            {
                _serviceEntryCache.UpdateRequestServiceEntryCache((path, httpMethod), serviceEntry);
            }

            return serviceEntry;
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