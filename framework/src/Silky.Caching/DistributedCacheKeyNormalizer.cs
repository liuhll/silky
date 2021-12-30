using Silky.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Caching.Configuration;
using Silky.Core.Runtime.Session;

namespace Silky.Caching
{
    public class DistributedCacheKeyNormalizer : IDistributedCacheKeyNormalizer, ITransientDependency
    {
        protected ISession Session { get; }

        protected SilkyDistributedCacheOptions DistributedCacheOptions { get; private set; }


        public DistributedCacheKeyNormalizer(
            IOptionsMonitor<SilkyDistributedCacheOptions> distributedCacheOptions)
        {
            DistributedCacheOptions = distributedCacheOptions.CurrentValue;
            Session = NullSession.Instance;
            distributedCacheOptions.OnChange((options, s) => DistributedCacheOptions = options);
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