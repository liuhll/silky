using System.Collections.Generic;
using System.Linq;

namespace Silky.Core.Configuration;

public class AppServicePlugInSourceList : List<ServicePlugInOption>
{
    public ServicePlugInOption[] GetAppServiceSources()
    {
        var appSettingsOptions =
            EngineContext.Current.GetOptions<PlugInSourceOptions>(PlugInSourceOptions.PlugInSource);
        if (appSettingsOptions.AppServicePlugIns != null)
        {
            AddRange(appSettingsOptions.AppServicePlugIns);
        }
        return this.Distinct().ToArray();
    }
}