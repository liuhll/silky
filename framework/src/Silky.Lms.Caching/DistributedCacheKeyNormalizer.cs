using Silky.Lms.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using Silky.Lms.Caching.Configuration;

namespace Silky.Lms.Caching
{
    public class DistributedCacheKeyNormalizer : IDistributedCacheKeyNormalizer, ITransientDependency
    {
        //protected ICurrentTenant CurrentTenant { get; }

        protected LmsDistributedCacheOptions DistributedCacheOptions { get; }

        public DistributedCacheKeyNormalizer(
            IOptions<LmsDistributedCacheOptions> distributedCacheOptions)
        {
            DistributedCacheOptions = distributedCacheOptions.Value;
        }

        public virtual string NormalizeKey(DistributedCacheKeyNormalizeArgs args)
        {
            var normalizedKey = $"c:{args.CacheName},k:{DistributedCacheOptions.KeyPrefix}{args.Key}";
            return normalizedKey;
        }
    }
}