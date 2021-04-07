using System;
using Microsoft.Extensions.Caching.Distributed;

namespace Silky.Lms.Caching.Configuration
{
    public class LmsDistributedCacheOptions
    {
        public static string LmsDistributedCache = "DistributedCache";

        public LmsDistributedCacheOptions()
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