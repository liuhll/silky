using System;
using Microsoft.Extensions.Caching.Distributed;

namespace Silky.Caching.Configuration
{
    public class SilkyDistributedCacheOptions
    {
        internal static string SilkyDistributedCache = "DistributedCache";

        public SilkyDistributedCacheOptions()
        {
            HideErrors = true;
            KeyPrefix = "";
            GlobalCacheEntryOptions = new();
            GlobalCacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(20);
            Redis = new RedisOptions();
        }

        public bool HideErrors { get; set; }
        public string KeyPrefix { get; set; }

        public RedisOptions Redis { get; set; }
        public DistributedCacheEntryOptions GlobalCacheEntryOptions { get; set; }
    }
}