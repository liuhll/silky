using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Silky.Core.Modularity;

public class ApplicationShutdownContext
{
    public IServiceProvider ServiceProvider { get; private set; }
    public IHostEnvironment HostEnvironment { get; private set; }

    internal ApplicationShutdownContext(
        [NotNull] IServiceProvider serviceProvider,
        [NotNull] IHostEnvironment hostEnvironment)
    {
        Check.NotNull(serviceProvider, nameof(serviceProvider));
        Check.NotNull(hostEnvironment, nameof(hostEnvironment));
        ServiceProvider = serviceProvider;
        HostEnvironment = hostEnvironment;
    }
}