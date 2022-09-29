using System.Collections.Generic;
using System.Linq;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class ClientFilterProvider : ISingletonDependency
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public ClientFilterProvider(IServiceEntryLocator serviceEntryLocator)
        {
            _serviceEntryLocator = serviceEntryLocator;
        }

        public IClientFilter[] GetClientFilters(string serviceEntryId)
        {
            var clientFilters = new List<IClientFilter>();
            var globalFilter = EngineContext.Current.ResolveAll<IClientFilter>();
            clientFilters.AddRange(globalFilter);

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryId);
            if (serviceEntry != null)
            {
                clientFilters.AddRange(serviceEntry.ClientFilters);
            }

            var filters = clientFilters.OrderBy(p => p.Order).ToArray();
            return filters;
        }
    }
}