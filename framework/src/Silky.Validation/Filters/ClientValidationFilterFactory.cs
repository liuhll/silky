using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Core.DependencyInjection;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;
using IServiceProvider = System.IServiceProvider;

namespace Silky.Validation.Filters;

public class ClientValidationFilterFactory : IClientFilterFactory, ISingletonDependency
{
    public bool IsReusable { get; } = true;

    public IClientFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new ClientValidationFilter(serviceProvider.GetRequiredService<IMethodInvocationValidator>(),
            serviceProvider.GetRequiredService<IServiceEntryLocator>());
    }
}