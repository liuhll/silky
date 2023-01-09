using System;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.DependencyInjection;
using Silky.Core.Serialization;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Client.Filters;

public class AsyncAlwaysRemoteFileResultFilterFactory : IClientFilterFactory, ISingletonDependency
{
    public bool IsReusable { get; } = true;
    
    public IClientFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new AsyncAlwaysRemoteFileResultFilter(serviceProvider.GetRequiredService<ISerializer>());
    }
}