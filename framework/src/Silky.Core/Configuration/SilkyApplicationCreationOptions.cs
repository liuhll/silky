using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Silky.Core.Modularity.PlugIns;

namespace Silky.Core.Configuration;

public class SilkyApplicationCreationOptions
{
    [NotNull] public PlugInSourceList ModulePlugInSources { get; }

    [NotNull] public AppServicePlugInSourceList AppServicePlugInSources { get; }

    [NotNull] public FilterOptions Filter { get; }
    public bool DisplayFullErrorStack { get; set; }

    public bool LogEntityFrameworkCoreSqlExecuteCommand { get; set; }

    public bool AutoValidationParameters { get; set; }

    public bool IgnoreCheckingRegisterType { get; set; }

    public bool UsingServiceShortName { get; set; }

    public string ServiceTemplateName { get; set; }

    public string? ApplicationName { get; set; }

    public BannerMode BannerMode { get; set; }

    public string BannerContent { get; set; }

    public string DocUrl { get; set; }

    public SilkyConfigurationBuilderOptions Configuration { get; set; }
    
    public bool GlobalAuthorize { get; set; }

    public SilkyApplicationCreationOptions()
    {
        DisplayFullErrorStack = false;
        AutoValidationParameters = true;
        LogEntityFrameworkCoreSqlExecuteCommand = false;
        IgnoreCheckingRegisterType = false;
        ModulePlugInSources = new PlugInSourceList();
        AppServicePlugInSources = new AppServicePlugInSourceList();
        Configuration = new SilkyConfigurationBuilderOptions();
        Filter = new FilterOptions();
        ApplicationName = Assembly.GetEntryAssembly()?.GetName()?.Name.Split(".").Last();
        BannerMode = BannerMode.CONSOLE;
        GlobalAuthorize = false;
        UsingServiceShortName = true;
        ServiceTemplateName = "AppService";
        DocUrl = "https://docs.silky-fk.com";
    }
}