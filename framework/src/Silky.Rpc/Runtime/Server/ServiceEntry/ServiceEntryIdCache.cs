using System.Collections.Concurrent;
using System.Reflection;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntryIdCache : ISingletonDependency
    {
        private ConcurrentDictionary<MethodInfo, string> m_serviceEntryIdCache = new();

        public bool TryGetServiceEntryId(MethodInfo method, out string serviceEntryId)
        {
            return m_serviceEntryIdCache.TryGetValue(method, out serviceEntryId);
        }

        public void UpdateServiceEntryIdCache(MethodInfo method, string serviceEntryId)
        {
            m_serviceEntryIdCache.AddOrUpdate(method, serviceEntryId, (k, _) => serviceEntryId);
        }
    }
}