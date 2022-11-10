using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity.PlugIns;

namespace Silky.Core.Configuration;

public class SilkyApplicationCreationOptions
{
    [NotNull]
    public IServiceCollection Services { get; }

    [NotNull]
    public PlugInSourceList PlugInSources { get; }
    
    public string ApplicationName { get; set; }

    public SilkyConfigurationBuilderOptions Configuration { get; set; }

    public SilkyApplicationCreationOptions([NotNull] IServiceCollection services)
    {
        Services = Check.NotNull(services, nameof(services));
        PlugInSources = new PlugInSourceList();
        Configuration = new SilkyConfigurationBuilderOptions();
    }
    
}