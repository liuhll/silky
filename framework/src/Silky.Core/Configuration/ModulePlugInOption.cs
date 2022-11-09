using System;

namespace Silky.Core.Configuration;

public class ModulePlugInOption
{
    public ModulePlugInOption()
    {
        Folders = Array.Empty<ModuleFolderPlugInOption>();
        FilePaths = Array.Empty<string>();
        Types = Array.Empty<string>();
    }

    public ModuleFolderPlugInOption[] Folders { get; set; }

    public string[] FilePaths { get; set; }

    public string[] Types { get; set; }
}