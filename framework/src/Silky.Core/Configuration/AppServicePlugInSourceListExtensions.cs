using JetBrains.Annotations;

namespace Silky.Core.Configuration;

public static class AppServicePlugInSourceListExtensions
{
    public static void AddFolders(
        [NotNull] this AppServicePlugInSourceList list,
        params ServicePlugInOption[] servicePlugInOption)
    {
        Check.NotNull(list, nameof(list));
        foreach (var folderPlugInOption in servicePlugInOption)
        {
            list.Add(folderPlugInOption);
        }
    }
}