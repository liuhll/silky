using System.IO;

namespace Silky.Core.Configuration;

public class ServicePlugInOption : FolderPlugInOption
{
    public ServicePlugInOption()
    {
        Pattern = "Application|Service";
        SearchOption = SearchOption.TopDirectoryOnly;
    }
}