using Silky.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Caching.Configuration;

namespace Silky.Caching
{
    public class DistributedCacheKeyNormalizer : IDistributedCacheKeyNormalizer, ITransientDependency
    {
        //protected ICurrentTenant CurrentTenant { get; }

        protected SilkyDistributedCacheOptions DistributedCacheOptions { get; }

        public DistributedCacheKeyNormalizer(
            IOptions<SilkyDistributedCacheOptions> distributedCacheOptions)
        {
            DistributedCacheOptions = distributedCacheOptions.Value;
        }

        public virtual string NormalizeKey(DistributedCacheKeyNormalizeArgs args)
        {
            if (args.Key.Contains($"c:{args.CacheName}") && args.Key.Contains("k:"))
            {
                return args.Key;
            }

            var normalizedKey = $"c:{args.CacheName},k:{DistributedCacheOptions.KeyPrefix}{args.Key}";
            return normalizedKey;
        }
    }
}