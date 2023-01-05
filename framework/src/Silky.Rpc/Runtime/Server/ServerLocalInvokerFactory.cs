using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Server;

internal class ServerLocalInvokerFactory : IServerLocalInvokerFactory, ISingletonDependency
{
    private readonly ILogger _logger;
    private readonly IServerFilterProvider _serverFilterProvider;
    private readonly IServiceEntryContextAccessor _serviceEntryContextAccessor;
    private ConcurrentDictionary<string, FilterItem[]> _cacheFilters = new(); 

    public ServerLocalInvokerFactory(ILoggerFactory loggerFactory,
        IServerFilterProvider serverFilterProvider,
        IServiceEntryContextAccessor? serviceEntryContextAccessor)
    {
        _serverFilterProvider = serverFilterProvider;
        _serviceEntryContextAccessor = serviceEntryContextAccessor ?? ServiceEntryContextAccessor.Null;
      
        _logger = loggerFactory.CreateLogger<LocalInvoker>();
    }

    public LocalInvoker CreateInvoker(ServiceEntryContext serviceEntryContext)
    {
        if (serviceEntryContext == null)
            throw new ArgumentNullException(nameof(serviceEntryContext));
        
        IServerFilterMetadata[] filters;

        if (!_cacheFilters.TryGetValue(serviceEntryContext.ServiceEntry.Id,out var cachedFilterItems))
        {
            var filterFactoryResult = FilterFactory.GetAllServerFilters(_serverFilterProvider, serviceEntryContext);
            filters = (IServerFilterMetadata[])filterFactoryResult.Filters;
            _cacheFilters.TryAdd(serviceEntryContext.ServiceEntry.Id, filterFactoryResult.CacheableFilters);
        }
        else
        {
            filters = FilterFactory.CreateUncachedFilters(_serverFilterProvider, serviceEntryContext, cachedFilterItems);
        }
        var invoker = new ServiceEntryLocalInvoker(_logger,serviceEntryContext,_serviceEntryContextAccessor, filters);
        return invoker;
    }
}  