using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using Silky.Core.DependencyInjection;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Client;

public class DefaultClientRemoteInvokerFactory : IClientRemoteInvokerFactory, ISingletonDependency
{
    private readonly ILogger _logger;
    private readonly IFilterProvider _filterProvider;
    private ConcurrentDictionary<string, FilterItem[]> _cacheFilters = new();
    private readonly IClientInvokeContextAccessor _clientInvokeContextAccessor;
    private readonly IClientFilterDescriptorProvider _clientFilterDescriptorProvider;
    private readonly IServerManager _serverManager;

    public DefaultClientRemoteInvokerFactory(ILoggerFactory loggerFactory,
        IFilterProvider filterProvider,
        IClientInvokeContextAccessor? clientInvokeContextAccessor,
        IClientFilterDescriptorProvider clientFilterDescriptorProvider,
        IServerManager serverManager)
    {
        _filterProvider = filterProvider;
        _clientFilterDescriptorProvider = clientFilterDescriptorProvider;
        _serverManager = serverManager;
        _clientInvokeContextAccessor = clientInvokeContextAccessor ?? ClientInvokeContextAccessor.Null;

        _logger = loggerFactory.CreateLogger<RemoteInvokerBase>();
    }

    public IRemoteInvoker CreateInvoker(ClientInvokeContext context, ITransportClient client, string messageId)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var clientFilterDescriptors =
            _clientFilterDescriptorProvider.GetClientFilters(context.RemoteInvokeMessage.ServiceEntryId);
        IClientFilterMetadata[] filters;
        if (!_cacheFilters.TryGetValue(context.RemoteInvokeMessage.ServiceEntryId, out var cachedFilterItems))
        {
            var filterFactoryResult =
                ClientFilterFactory.GetAllFilters(_filterProvider, clientFilterDescriptors.ToArray());
            filters = (IClientFilterMetadata[])filterFactoryResult.Filters;
            _cacheFilters.TryAdd(context.RemoteInvokeMessage.ServiceEntryId, filterFactoryResult.CacheableFilters);
        }
        else
        {
            filters = ClientFilterFactory.CreateUncachedFilters(_filterProvider, cachedFilterItems);
        }

        var serviceEntryDescriptor =
            _serverManager.GetServiceEntryDescriptor(context.RemoteInvokeMessage.ServiceEntryId);

        var invoker = new RemoteInvoker(_logger, context, _clientInvokeContextAccessor, messageId,
            client, filters, serviceEntryDescriptor.GovernanceOptions.TimeoutMillSeconds);
        return invoker;
    }
}