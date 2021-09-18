using System.Linq;
using Silky.Core.MiniProfiler;

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
            MiniProfileConstant.MiniProfilerRouteBasePath
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