using System.IO;

namespace Silky.Core.Configuration;

public class ModuleFolderPlugInOption : FolderPlugInOption
{
    public ModuleFolderPlugInOption()
    {
        Pattern = null;
        SearchOption = SearchOption.TopDirectoryOnly;
    }
}