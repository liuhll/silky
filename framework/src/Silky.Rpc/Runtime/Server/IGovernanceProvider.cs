using Silky.Rpc.Address.Selector;

namespace Silky.Rpc.Runtime.Server
{
    public interface IGovernanceProvider
    {
        /// <summary>
        /// 负载分流策略
        /// </summary>
        AddressSelectorMode ShuntStrategy { get; set; }

        /// <summary>
        /// 执行超时时间
        /// </summary>
        int TimeoutMillSeconds { get; set; }

        /// <summary>
        /// 是否开启缓存拦截
        /// </summary>
        bool EnableCachingInterceptor { get; set; }


        /// <summary>
        /// 是否开启熔断保护
        /// </summary>
        bool EnableCircuitBreaker { get; set; }

        /// <summary>
        /// 熔断时长
        /// </summary>
        int BreakerMillSeconds { get; set; }

        /// <summary>
        /// 熔断前允许出现的异常
        /// </summary>

        int ExceptionsAllowedBeforeBreaking { get; set; }

        /// <summary>
        /// 故障转移次数
        /// </summary>
        int RetryTimes { get; set; }

        int RetryIntervalMillSeconds { get; set; }

        /// <summary>
        /// 当个服务允许的最大处理
        /// </summary>
        int ConcurrentProcessingtCount { get; set; }
    }
}