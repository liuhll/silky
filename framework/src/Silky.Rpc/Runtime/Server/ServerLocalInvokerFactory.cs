using System;
using Microsoft.Extensions.Logging;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Server;

internal class ServerLocalInvokerFactory : IServerLocalInvokerFactory, ISingletonDependency
{
    private readonly ILogger _logger;
    private readonly IServerFilterProvider _serverFilterProvider;
    private readonly IServiceEntryContextAccessor _serviceEntryContextAccessor;

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
        var filterFactoryResult = FilterFactory.GetAllServerFilters(_serverFilterProvider, serviceEntryContext);
        var invoker = new ServiceEntryLocalInvoker(_logger,serviceEntryContext,_serviceEntryContextAccessor, filterFactoryResult.Filters);
        return invoker;
    }
}  