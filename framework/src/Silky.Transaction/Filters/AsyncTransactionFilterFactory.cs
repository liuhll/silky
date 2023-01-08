using Microsoft.Extensions.DependencyInjection;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;
using IServiceProvider = System.IServiceProvider;

namespace Silky.Transaction.Filters;

public class AsyncTransactionFilterFactory : IClientFilterFactory, ISingletonDependency
{
    public bool IsReusable { get; } = true;
    public IClientFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new AsyncTransactionFilter(serviceProvider.GetRequiredService<IServiceEntryLocator>(),
            serviceProvider.GetRequiredService<IServerManager>());
    }
}