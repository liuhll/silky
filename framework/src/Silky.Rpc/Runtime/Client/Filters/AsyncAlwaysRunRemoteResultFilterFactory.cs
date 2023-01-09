using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Convertible;
using Silky.Core.DependencyInjection;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;
using IServiceProvider = System.IServiceProvider;

namespace Silky.Rpc.Runtime.Client.Filters;

public class AsyncAlwaysRunRemoteResultFilterFactory : IClientFilterFactory, ISingletonDependency
{
    public bool IsReusable { get; } = true;

    public IClientFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new AsyncAlwaysRunRemoteResultFilter(serviceProvider.GetRequiredService<IServiceEntryLocator>(),
            serviceProvider.GetRequiredService<ITypeConvertibleService>());
    }
}