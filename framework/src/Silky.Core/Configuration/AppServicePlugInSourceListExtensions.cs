using System.IO;
using JetBrains.Annotations;

namespace Silky.Core.Configuration;

public static class AppServicePlugInSourceListExtensions
{
    public static void AddFolders(
        [NotNull] this AppServicePlugInSourceList list,
        params ServicePlugInOption[] servicePlugInOptions)
    {
        Check.NotNull(list, nameof(list));
        list.AddRange(servicePlugInOptions);
    }

    public static void AddFolder(
        [NotNull] this AppServicePlugInSourceList list,[NotNull] string folder, string pattern = "Application|Service",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        Check.NotNull(list, nameof(list));
        Check.NotNull(folder, nameof(folder));
        list.Add(new ServicePlugInOption()
        {
            Folder = folder,
            Pattern = pattern,
            SearchOption = searchOption
        });
    }
}