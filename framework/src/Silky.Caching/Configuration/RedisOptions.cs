namespace Silky.Caching.Configuration
{
    public class RedisOptions
    {
        public RedisOptions()
        {
            Configuration = "";
            IsEnabled = false;
        }

        public string Configuration { get; set; }

        public bool IsEnabled { get; set; }
    }
}