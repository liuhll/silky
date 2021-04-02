using System.Linq;
using Lms.Rpc.Runtime.Server.Parameter;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Runtime.Server
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

        public ServiceEntry GetServiceEntryById(string serviceId)
        {
            if (_serviceEntryCache.TryGetServiceEntry(serviceId, out ServiceEntry serviceEntry))
            {
                return serviceEntry;
            }

            serviceEntry = _serviceEntryManager.GetAllEntries()
                .FirstOrDefault(p => p.ServiceDescriptor.Id == serviceId);
            if (serviceEntry != null)
            {
                _serviceEntryCache.UpdateServiceEntryCache(serviceEntry);
            }

            return serviceEntry;
        }

        public ServiceEntry GetLocalServiceEntryById(string serviceId)
        {
            if (_serviceEntryCache.TryGetLocalServiceEntry(serviceId, out ServiceEntry serviceEntry))
            {
                return serviceEntry;
            }

            serviceEntry = _serviceEntryManager.GetLocalEntries()
                .FirstOrDefault(p => p.ServiceDescriptor.Id == serviceId);
            if (serviceEntry != null)
            {
                _serviceEntryCache.UpdateLocalServiceEntryCache(serviceEntry);
            }

            return serviceEntry;
        }
    }
}