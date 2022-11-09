using System.IO;

namespace Silky.Core.Configuration;

public abstract class FolderPlugInOption
{
    public string Folder { get; set; }
    
    public SearchOption SearchOption { get; set; }
    
    public string Pattern { get; set; }
}