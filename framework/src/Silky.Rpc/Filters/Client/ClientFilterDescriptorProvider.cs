using System.Collections.Concurrent;
using System.Collections.Generic;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

public class ClientFilterDescriptorProvider : IClientFilterDescriptorProvider, ISingletonDependency
{
    private readonly IServiceEntryLocator _serviceEntryLocator;

    private ConcurrentDictionary<string, FilterDescriptor[]> _clientFilterCache = new();

    public ClientFilterDescriptorProvider(IServiceEntryLocator serviceEntryLocator)
    {
        _serviceEntryLocator = serviceEntryLocator;
    }

    public FilterDescriptor[] GetClientFilters(string serviceEntryId)
    {
        if (_clientFilterCache.TryGetValue(serviceEntryId,out var filters))
        {
            return filters;
        }

        var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryId);
        if (serviceEntry != null)
        {
            filters = serviceEntry.ClientFilters;
        }
        else
        {
            var clientFilterFactories = EngineContext.Current.ResolveAll<IClientFilterFactory>();
            var filterDescriptors = new List<FilterDescriptor>();
            foreach (var clientFilterFactory in clientFilterFactories)
            {
                filterDescriptors.Add(new FilterDescriptor(clientFilterFactory, FilterScope.Global));
            }
            filterDescriptors.Add(new FilterDescriptor(new RemoteInvokeBehavior(), FilterScope.ServiceEntry));
            filters = filterDescriptors.ToArray();
        }
        _clientFilterCache.TryAdd(serviceEntryId, filters);
        return filters;
    }
}