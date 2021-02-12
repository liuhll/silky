using System;
using System.Net.Mail;
using System.Threading;
using Lms.Rpc.Address.Selector;

namespace Lms.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class GovernanceAttribute : Attribute, IGovernanceProvider
    {
        /// <summary>
        /// 负载分流策略
        /// </summary>
        public AddressSelectorMode ShuntStrategy { get; set; } = AddressSelectorMode.Polling;
        
        /// <summary>
        /// 执行超时时间
        /// </summary>
        public int ExecutionTimeout { get; set; } = Timeout.Infinite;

        /// <summary>
        /// 是否开启缓存拦截
        /// </summary>
        public bool CacheEnabled { get; set; } = true;
        
        /// <summary>
        /// 允许的最大并发量
        /// </summary>
        public int MaxConcurrent { get; set; } = 10;

        /// <summary>
        /// 熔断休眠时间
        /// </summary>
        public int FuseSleepDuration { get; set; } = 60;

        /// <summary>
        /// 是否开启熔断保护
        /// </summary>
        public bool FuseProtection { get; set; } = true;

        /// <summary>
        /// 故障转移次数
        /// </summary>
        public int FailoverCount { get; set; } = 0;
    }
}