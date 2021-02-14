using Lms.Rpc.Address.Selector;

namespace Lms.Rpc.Runtime.Server
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
        int ExecutionTimeout { get; set; }

        /// <summary>
        /// 是否开启缓存拦截
        /// </summary>
        bool CacheEnabled { get; set; }
        
        /// <summary>
        /// 允许的最大并发量
        /// </summary>
        int MaxConcurrent { get; set; }

        /// <summary>
        /// 熔断休眠时间
        /// </summary>
        int FuseSleepDuration { get; set; }

        /// <summary>
        /// 是否开启熔断保护
        /// </summary>
        bool FuseProtection { get; set; }
        
        /// <summary>
        /// 故障转移次数
        /// </summary>
        int FailoverCount { get; set; }
        
        
    }
}