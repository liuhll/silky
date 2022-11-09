using System.Collections.Generic;

namespace Silky.Core.Configuration;

public class ModulePlugInOption
{
    public ModulePlugInOption()
    {
        Folders = new List<ModuleFolderPlugInOption>();
        FilePaths = new List<string>();
        Types = new List<string>();
    }

    public ICollection<ModuleFolderPlugInOption> Folders { get; set; }

    public ICollection<string> FilePaths { get; set; }
    
    public ICollection<string> Types { get; set; }
}