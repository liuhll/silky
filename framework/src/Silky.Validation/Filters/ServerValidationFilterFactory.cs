using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Core.Configuration;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Filters;

namespace Silky.Validation.Filters;

public class ServerValidationFilterFactory : IServerFilterFactory, ISingletonDependency
{
    public bool IsReusable { get; } = true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new ServerValidationFilter(serviceProvider.GetRequiredService<IMethodInvocationValidator>(),
            serviceProvider.GetRequiredService<IOptionsMonitor<AppSettingsOptions>>());
    }
}