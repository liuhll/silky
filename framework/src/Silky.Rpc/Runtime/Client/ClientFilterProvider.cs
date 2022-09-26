using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class ClientFilterProvider : IScopedDependency
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;

        private ConcurrentDictionary<string, IClientFilter[]> _filterCaches = new();


        public ClientFilterProvider(IServiceEntryLocator serviceEntryLocator)
        {
            _serviceEntryLocator = serviceEntryLocator;
        }

        public IClientFilter[] GetClientFilters(string serviceEntryId)
        {
            if (_filterCaches.TryGetValue(serviceEntryId, out var filters))
            {
                return filters;
            }

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryId);
            if (serviceEntry == null)
            {
                return Array.Empty<IClientFilter>();
            }

            var clientFilters = new List<IClientFilter>();
            var globalFilter = EngineContext.Current.ResolveAll<IClientFilter>().OrderBy(p => p.Order).ToArray();
            clientFilters.AddRange(globalFilter);
            clientFilters.AddRange(serviceEntry.ClientFilters);
            filters = clientFilters.OrderBy(p => p.Order).ToArray();
            _filterCaches.TryAdd(serviceEntryId, filters);
            return filters;
        }
    }
}