using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Configuration
{
    public class GovernanceOptions : IGovernanceProvider
    {
        internal static string Governance = "Governance";

        public GovernanceOptions()
        {
            ShuntStrategy = ShuntStrategy.Polling;
            TimeoutMillSeconds = 5000;
            EnableCachingInterceptor = true;
            EnableCircuitBreaker = true;
            ExceptionsAllowedBeforeBreaking = 3;
            BreakerSeconds = 60;
            AddressFuseSleepDurationSeconds = 60;
            UnHealthAddressTimesAllowedBeforeRemoving = 3;
            RetryIntervalMillSeconds = 50;
            RetryTimes = 3;
            MaxConcurrentHandlingCount = 50;
            _heartbeatWatchIntervalSeconds = 300;
            EnableHeartbeat = false;
            ApiIsRESTfulStyle = true;
            
        }


        /// <summary>
        /// 负载分流策略
        /// </summary>
        public ShuntStrategy ShuntStrategy { get; set; }


        /// <summary>
        /// Rpc调用执行超时时间
        /// </summary>
        public int TimeoutMillSeconds { get; set; }

        /// <summary>
        /// 是否开启缓存拦截
        /// </summary>
        public bool EnableCachingInterceptor { get; set; }

        /// <summary>
        /// 熔断休眠时长
        /// </summary>
        public int AddressFuseSleepDurationSeconds { get; set; }

        /// <summary>
        /// 地址被标识不健康多少次后会被移除
        /// </summary>
        public int UnHealthAddressTimesAllowedBeforeRemoving { get; set; }

        /// <summary>
        /// 是否开启熔断保护,用户友好类异常不会触发熔断保护
        /// </summary>
        public bool EnableCircuitBreaker { get; set; }

        /// <summary>
        /// 熔断时长
        /// </summary>
        public int BreakerSeconds { get; set; }

        /// <summary>
        /// 熔断前允许出现的异常
        /// </summary>
        public int ExceptionsAllowedBeforeBreaking { get; set; }

        /// <summary>
        /// 故障转移次数
        /// </summary>
        public int RetryTimes { get; set; }

        /// <summary>
        /// 故障转移间隔时间
        /// </summary>
        public int RetryIntervalMillSeconds { get; set; }

        public int MaxConcurrentHandlingCount { get; set; }

        public bool EnableHeartbeat { get; set; }

        private int _heartbeatWatchIntervalSeconds;
        public bool ApiIsRESTfulStyle { get; set; }

        public int HeartbeatWatchIntervalSeconds
        {
            get => _heartbeatWatchIntervalSeconds;
            set => _heartbeatWatchIntervalSeconds = value <= 60 ? 60 : value;
        }
    }
}