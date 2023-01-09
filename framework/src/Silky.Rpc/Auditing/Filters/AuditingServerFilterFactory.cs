using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Configuration;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Auditing.Filters;

public class AuditingServerFilterFactory : IServerFilterFactory, ISingletonDependency
{
    public bool IsReusable { get; } = false;
    
    public IServerFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new AuditingServerFilter(serviceProvider.GetService<IOptions<AuditingOptions>>(),
            serviceProvider.GetService<IAuditSerializer>());
    }
}