using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Reflection;

namespace Silky.Core.Modularity.PlugIns;

public class FolderPlugInSource : IPlugInSource
{
    public string Folder { get; }

    public SearchOption SearchOption { get; set; }

    public string Pattern { get; set; }

    public ILogger<FolderPlugInSource> Logger { get; set; }

    public FolderPlugInSource(
        [NotNull] string folder,
        string pattern = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        Check.NotNull(folder, nameof(folder));

        Folder = folder;
        Pattern = pattern;
        SearchOption = searchOption;
        Logger = NullLogger<FolderPlugInSource>.Instance;
    }


    public Type[] GetModules()
    {
        var modules = new List<Type>();
        foreach (var assembly in GetAssemblies())
        {
            try
            {
                var moduleType = assembly.GetTypes().FirstOrDefault(type => SilkyModule.IsSilkyPluginModule(type));
                if (moduleType != null)
                {
                    modules.AddIfNotContains(moduleType);
                }
            }
            catch (Exception ex)
            {
                throw new SilkyException("Could not get module types from assembly: " + assembly.FullName, ex);
                // Logger.LogWarning("Could not get module types from assembly: " + assembly.FullName);
            }
        }

        return modules.ToArray();
    }

    private IEnumerable<Assembly> GetAssemblies()
    {
        var assemblyFiles = AssemblyHelper.GetAssemblyFiles(Folder, SearchOption);

        if (!Pattern.IsNullOrEmpty())
        {
            assemblyFiles = assemblyFiles.Where(a => AssemblyHelper.Matches(a, Pattern));
        }

        var assemblies = assemblyFiles.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
            .Where(a => AssemblyHelper.Matches(a.FullName))
            ;

        return assemblies;
    }
}