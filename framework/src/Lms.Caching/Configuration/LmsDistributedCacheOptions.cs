namespace Lms.Caching.Configuration
{
    public class LmsDistributedCacheOptions
    {
        public bool HideErrors { get; set; } = true;
        public string KeyPrefix { get; set; }
    }
}