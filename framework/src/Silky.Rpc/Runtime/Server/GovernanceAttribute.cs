using System;
using System.Threading;
using JetBrains.Annotations;
using Silky.Rpc.Address.Selector;


namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class GovernanceAttribute : Attribute, IGovernanceProvider
    {
        /// <summary>
        /// Shunt Strategy
        /// </summary>
        public AddressSelectorMode ShuntStrategy { get; set; } = AddressSelectorMode.Polling;

        /// <summary>
        /// Execution timeout
        /// </summary>
        public int ExecutionTimeout { get; set; } = 5000;

        /// <summary>
        /// Whether to enable cache interception
        /// </summary>
        public bool CacheEnabled { get; set; } = true;

        /// <summary>
        /// Maximum allowed concurrency
        /// </summary>
        public int MaxConcurrent { get; set; } = 10;

        /// <summary>
        /// Fuse sleep time
        /// </summary>
        public int FuseSleepDuration { get; set; } = 60;

        /// <summary>
        /// Whether to open the fuse protection
        /// </summary>
        public bool FuseProtection { get; set; } = true;

        /// <summary>
        /// Number of failovers
        /// </summary>
        public int FailoverCount { get; set; } = 0;

        /// <summary>
        /// The type specified by the failure callback
        /// </summary>
        [CanBeNull] public Type FallBackType { get; set; }
        
        /// <summary>
        /// Whether to prohibit external network access
        /// </summary>
        public bool ProhibitExtranet{ get; set; }
    }
}