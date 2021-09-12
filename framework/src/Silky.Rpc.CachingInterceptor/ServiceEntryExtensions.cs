using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Silky.Caching;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.CachingInterceptor
{
    public static class ServiceEntryExtensions
    {
        public static string GetCacheName([NotNull] this ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            var returnType = serviceEntry.ReturnType;
            var cacheNameAttribute = returnType.GetCustomAttributes().OfType<CacheNameAttribute>().FirstOrDefault();
            if (cacheNameAttribute != null)
            {
                return cacheNameAttribute.Name;
            }

            return returnType.FullName.RemovePostFix("CacheItem");
        }
    }
}