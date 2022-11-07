using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Swagger.Abstraction;

public class SwaggerGroupUtils
{
    public static IEnumerable<string> ReadGroups(IEnumerable<Assembly> applicationInterfaceAssemblies)
    {
        var groups = new List<string>();
        groups.AddRange(applicationInterfaceAssemblies
            .Select(p => p.GetName().Name));
        return groups;
    }
    
    public static IEnumerable<string> ReadLocalGroups()
    {
        var groups = new List<string>();
        groups.AddRange(ServiceHelper.ReadInterfaceAssemblies()
            .Select(p => p.GetName().Name));
        return groups;
    }

    public static IEnumerable<(string, string)> ReadGroupInfos(IEnumerable<Assembly> applicationInterfaceAssemblies)
    {
        var groups = new List<(string, string)>();
        groups.AddRange(applicationInterfaceAssemblies
            .Select(p =>
            {
                var version = p.GetName().Version;
                var ver = $"{version.Major}.{version.Minor}.{version.Build}";
                return (p.GetName().Name, ver);
            }));
        return groups;
    }
}