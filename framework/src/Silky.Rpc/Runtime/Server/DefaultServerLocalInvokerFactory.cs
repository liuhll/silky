using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Silky.Core.DependencyInjection;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Server;

internal class DefaultServerLocalInvokerFactory : IServerLocalInvokerFactory, ISingletonDependency
{
    private readonly ILogger _logger;
    private readonly IFilterProvider _filterProvider;
    private readonly IServiceEntryContextAccessor _serviceEntryContextAccessor;
    private ConcurrentDictionary<string, FilterItem[]> _cacheFilters = new(); 

    public DefaultServerLocalInvokerFactory(ILoggerFactory loggerFactory,
        IFilterProvider filterProvider,
        IServiceEntryContextAccessor? serviceEntryContextAccessor)
    {
        _filterProvider = filterProvider;
        _serviceEntryContextAccessor = serviceEntryContextAccessor ?? ServiceEntryContextAccessor.Null;
      
        _logger = loggerFactory.CreateLogger<LocalInvokerBase>();
    }

    public ILocalInvoker CreateInvoker(ServiceEntryContext serviceEntryContext)
    {
        if (serviceEntryContext == null)
            throw new ArgumentNullException(nameof(serviceEntryContext));
        
        IServerFilterMetadata[] filters;

        if (!_cacheFilters.TryGetValue(serviceEntryContext.ServiceEntry.Id,out var cachedFilterItems))
        {
            var filterFactoryResult = ServerFilterFactory.GetAllFilters(_filterProvider, serviceEntryContext);
            filters = (IServerFilterMetadata[])filterFactoryResult.Filters;
            _cacheFilters.TryAdd(serviceEntryContext.ServiceEntry.Id, filterFactoryResult.CacheableFilters);
        }
        else
        {
            filters = ServerFilterFactory.CreateUncachedFilters(_filterProvider, cachedFilterItems);
        }
        var invoker = new LocalInvoker(_logger,serviceEntryContext,_serviceEntryContextAccessor, filters);
        return invoker;
    }
}  