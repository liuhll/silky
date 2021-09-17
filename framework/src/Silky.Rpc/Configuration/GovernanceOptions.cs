using Silky.Rpc.Address.Selector;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Configuration
{
    public class GovernanceOptions : IGovernanceProvider
    {
        internal static string Governance = "Governance";

        public GovernanceOptions()
        {
            ShuntStrategy = AddressSelectorMode.Polling;
            TimeoutMillSeconds = 5000;
            EnableCachingInterceptor = true;
            EnableCircuitBreaker = true;
            ExceptionsAllowedBeforeBreaking = 3;
            BreakerMillSeconds = 1000;
            AddressFuseSleepDurationSeconds = 600;
            RemovedUnHealthAddressTimes = 0;
            RetryIntervalMillSeconds = 50;
            RetryTimes = 0;
            FailoverCountEqualInstanceCount = true;
            ConcurrentProcessingtCount = 50;
            TotalConcurrentProcessingtCount = 500;
        }


        /// <summary>
        /// 负载分流策略
        /// </summary>
        public AddressSelectorMode ShuntStrategy { get; set; } 

        
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
        public int RemovedUnHealthAddressTimes { get; set; }

        /// <summary>
        /// 是否开启熔断保护(业务异常不会导致熔断)
        /// </summary>
        public bool EnableCircuitBreaker { get; set; }

        /// <summary>
        /// 熔断时长
        /// </summary>
        public int BreakerMillSeconds { get; set; }

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
        public int RetryIntervalMillSeconds{ get; set; }

        public int ConcurrentProcessingtCount { get; set; }
        
        public int TotalConcurrentProcessingtCount { get; set; }

        /// <summary>
        /// 故障转移次数与服务实例个数相同
        /// </summary>
        public bool FailoverCountEqualInstanceCount { get; set; }

    }
}