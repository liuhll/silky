using System;

namespace Silky.Lms.Lock.Configuration
{
    public class LockOptions
    {
        public static string Lock = "Lock";

        public string LockRedisConnection { get; set; }

        public int DefaultExpiry { get; set; } = 120;

        public int Wait { get; set; } = 30;

        public int Retry { get; set; } = 3;

        public TimeSpan DefaultExpiryTimeSpan => TimeSpan.FromSeconds(DefaultExpiry);

        public TimeSpan WaitTimeSpan => TimeSpan.FromSeconds(Wait);

        public TimeSpan RetryTimeSpan => TimeSpan.FromSeconds(Retry);
    }
}