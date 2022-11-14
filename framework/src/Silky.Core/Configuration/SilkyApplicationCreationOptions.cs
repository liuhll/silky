using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity.PlugIns;

namespace Silky.Core.Configuration;

public class SilkyApplicationCreationOptions
{
    [NotNull] public IServiceCollection Services { get; }

    [NotNull] public PlugInSourceList ModulePlugInSources { get; }

    [NotNull] public AppServicePlugInSourceList AppServicePlugInSources { get; }


    public string ApplicationName { get; set; }

    public BannerMode BannerMode { get; set; }

    public string BannerContent { get; set; }

    public string DocUrl { get; set; }

    public SilkyConfigurationBuilderOptions Configuration { get; set; }

    public SilkyApplicationCreationOptions([NotNull] IServiceCollection services)
    {
        Services = Check.NotNull(services, nameof(services));
        ModulePlugInSources = new PlugInSourceList();
        AppServicePlugInSources = new AppServicePlugInSourceList();
        Configuration = new SilkyConfigurationBuilderOptions();
        ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name;
        BannerMode = BannerMode.CONSOLE;
        DocUrl = "https://docs.silky-fk.com";
    }
}