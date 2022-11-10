using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using JetBrains.Annotations;
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

    public FolderPlugInSource(
        [NotNull] string folder,
        string pattern = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        Check.NotNull(folder, nameof(folder));

        Folder = folder;
        Pattern = pattern;
        SearchOption = searchOption;
    }


    public Type[] GetModules()
    {
        var modules = new List<Type>();
        foreach (var assembly in GetAssemblies())
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (SilkyModule.IsSilkyModule(type))
                    {
                        modules.AddIfNotContains(type);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SilkyException("Could not get module types from assembly: " + assembly.FullName, ex);
            }
        }

        return modules.ToArray();
    }

    private List<Assembly> GetAssemblies()
    {
        var assemblyFiles = AssemblyHelper.GetAssemblyFiles(Folder, SearchOption);

        if (!Pattern.IsNullOrEmpty())
        {
            assemblyFiles = assemblyFiles.Where(a => AssemblyHelper.Matches(a, Pattern));
        }

        return assemblyFiles.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).ToList();
    }
}