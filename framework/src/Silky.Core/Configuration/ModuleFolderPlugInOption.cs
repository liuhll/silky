using System.IO;

namespace Silky.Core.Configuration;

public class ModuleFolderPlugInOption : FolderPlugInOption
{
    public ModuleFolderPlugInOption()
    {
        Pattern = "Module";
        SearchOption = SearchOption.TopDirectoryOnly;
    }
}