using System;
using Microsoft.Extensions.Logging;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Server;

internal class ServerLocalInvokerFactory : IServerLocalInvokerFactory, ISingletonDependency
{
    private readonly ILogger _logger;
    private readonly IServerFilterProvider _serverFilterProvider;

    public ServerLocalInvokerFactory(ILoggerFactory loggerFactory,
        IServerFilterProvider serverFilterProvider)
    {
        _serverFilterProvider = serverFilterProvider;
        _logger = loggerFactory.CreateLogger<LocalInvoker>();
    }

    public ILocalInvoker CreateInvoker(ServiceEntryContext serviceEntryContext)
    {
        if (serviceEntryContext == null)
            throw new ArgumentNullException(nameof(serviceEntryContext));
        var filters = _serverFilterProvider.GetServerFilters(serviceEntryContext.ServiceEntry,
            serviceEntryContext.ServiceInstance.GetType());
        var invoker = new LocalInvoker(_logger,serviceEntryContext, filters);
        return invoker;
    }
}