using System;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.DependencyInjection;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;

namespace Silky.Validation.Filters;

public class ServerValidationFilterFactory : IServerFilterFactory, ISingletonDependency
{
    public bool IsReusable { get; } = true;

    public IServerFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new ServerValidationFilter(serviceProvider.GetRequiredService<IMethodInvocationValidator>());
    }
}