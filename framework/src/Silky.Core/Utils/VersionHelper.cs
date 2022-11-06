using System.Reflection;

namespace Silky.Core.Utils;

public class VersionHelper
{
    public static string GetSilkyVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var versionInfo = $"{version.Major}.{version.Minor}.{version.Build}";
        return versionInfo;
    }
    
    public static string GetCurrentVersion()
    {
        var version = Assembly.GetEntryAssembly().GetName().Version;
        var versionInfo = $"{version.Major}.{version.Minor}.{version.Build}";
        return versionInfo;
    }
}