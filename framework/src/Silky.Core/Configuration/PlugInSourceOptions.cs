using System.Collections.Generic;

namespace Silky.Core.Configuration;

public class PlugInSourceOptions
{
    public static string PlugInSource = "PlugInSource";

    public PlugInSourceOptions()
    {
        AppServicePlugIns = new List<ServicePlugInOption>();
    }

    public ICollection<ServicePlugInOption> AppServicePlugIns { get; set; }
 
    public ModulePlugInOption ModulePlugIn { get; set; }
}