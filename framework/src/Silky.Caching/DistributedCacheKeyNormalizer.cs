using Silky.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Caching.Configuration;
using Silky.Core.Runtime.Session;

namespace Silky.Caching
{
    public class DistributedCacheKeyNormalizer : IDistributedCacheKeyNormalizer, ISingletonDependency
    {
        protected ISession Session { get; }


        private IOptionsMonitor<SilkyDistributedCacheOptions> _distributedCacheOptionsMonitor;

        protected SilkyDistributedCacheOptions DistributedCacheOptions => _distributedCacheOptionsMonitor.CurrentValue;


        public DistributedCacheKeyNormalizer(
            IOptionsMonitor<SilkyDistributedCacheOptions> distributedCacheOptionsMonitor)
        {
            _distributedCacheOptionsMonitor = distributedCacheOptionsMonitor;
            Session = NullSession.Instance;
        }

        public virtual string NormalizeKey(DistributedCacheKeyNormalizeArgs args)
        {
            if (args.Key.Contains($"c:{args.CacheName}") && args.Key.Contains("k:"))
            {
                return args.Key;
            }

            var normalizedKey = $"c:{args.CacheName},k:{DistributedCacheOptions.KeyPrefix}{args.Key}";
            if (!args.IgnoreMultiTenancy && Session.TenantId != null)
            {
                normalizedKey = $"t:{Session.TenantId},{normalizedKey}";
            }

            return normalizedKey;
        }
    }
}