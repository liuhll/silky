using System;
using Microsoft.Extensions.Caching.Distributed;

namespace Lms.Caching.Configuration
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
        }

        public bool HideErrors { get; set; }
        public string KeyPrefix { get; set; }

        public DistributedCacheEntryOptions GlobalCacheEntryOptions { get; set; }
    }
}