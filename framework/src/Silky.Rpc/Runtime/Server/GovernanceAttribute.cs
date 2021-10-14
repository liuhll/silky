using System;
using Silky.Rpc.Endpoint.Selector;


namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class GovernanceAttribute : Attribute, IGovernanceProvider
    {
        public GovernanceAttribute()
        {
            ShuntStrategy = ShuntStrategy.Polling;
            TimeoutMillSeconds = 5000;
            EnableCachingInterceptor = true;
            EnableCircuitBreaker = true;
            ExceptionsAllowedBeforeBreaking = 3;
            BreakerSeconds = 60;
            RetryIntervalMillSeconds = 50;
            RetryTimes = 3;
        }

        /// <summary>
        /// Shunt Strategy
        /// </summary>
        public ShuntStrategy ShuntStrategy { get; set; }

        /// <summary>
        /// Execution timeout
        /// </summary>
        public int TimeoutMillSeconds { get; set; }

        /// <summary>
        /// Whether to enable cache interception
        /// </summary>
        public bool EnableCachingInterceptor { get; set; }


        /// <summary>
        /// Whether to open the circuit breaker
        /// </summary>
        public bool EnableCircuitBreaker { get; set; }

        public int BreakerSeconds { get; set; }

        public int ExceptionsAllowedBeforeBreaking { get; set; }

        /// <summary>
        /// Number of retry
        /// </summary>
        public int RetryTimes { get; set; }

        public int RetryIntervalMillSeconds { get; set; }

        /// <summary>
        /// Whether to prohibit external network access
        /// </summary>
        public bool ProhibitExtranet { get; set; }
    }
}