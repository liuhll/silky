using System.Linq;

namespace Silky.SkyApm.Diagnostics.Rpc.Http.Utils
{
    public static class PathUtils
    {
        public static readonly string[] StaticResourceExtensions = new[]
        {
            ".js",
            ".css",
            ".ico",
            ".jpg",
            ".png",
            ".gif",
            ".html",
            "/index-mini-profiler"
        };

        public static bool IsWebApiPath(string path)
        {
            if (StaticResourceExtensions.Any(path.Contains))
            {
                return false;
            }

            return true;
        }
    }
}