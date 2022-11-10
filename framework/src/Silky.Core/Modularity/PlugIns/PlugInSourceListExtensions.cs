using System;
using System.IO;
using JetBrains.Annotations;
using Silky.Core.Configuration;

namespace Silky.Core.Modularity.PlugIns;

public static class PlugInSourceListExtensions
{
    public static void AddFolder(
        [NotNull] this PlugInSourceList list,
        [NotNull] string folder,
        string pattern = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        Check.NotNull(list, nameof(list));

        list.Add(new FolderPlugInSource(folder, pattern, searchOption));
    }

    public static void AddFolders(
        [NotNull] this PlugInSourceList list,
        params FolderPlugInOption[] folderPlugInOptions)
    {
        Check.NotNull(list, nameof(list));
        foreach (var folderPlugInOption in folderPlugInOptions)
        {
            list.Add(new FolderPlugInSource(folderPlugInOption.Folder, folderPlugInOption.Pattern,
                folderPlugInOption.SearchOption));
        }
    }

    public static void AddTypeNames(
        [NotNull] this PlugInSourceList list,
        params string[] moduleTypeNames)
    {
        Check.NotNull(list, nameof(list));

        list.Add(new TypePlugInSource(moduleTypeNames));
    }

    public static void AddTypes(
        [NotNull] this PlugInSourceList list,
        params Type[] types)
    {
        Check.NotNull(list, nameof(list));
        list.Add(new TypePlugInSource(types));
    }

    public static void AddFiles(
        [NotNull] this PlugInSourceList list,
        params string[] filePaths)
    {
        Check.NotNull(list, nameof(list));

        list.Add(new FilePlugInSource(filePaths));
    }
}