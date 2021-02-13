using JetBrains.Annotations;
using Lms.Rpc.Configuration;

namespace Lms.Rpc.Runtime.Server
{
    public class ServiceEntryGovernance : GovernanceOptions
    {
        /// <summary>
        /// 失败回调指定的FallBack
        /// </summary>
        [CanBeNull] public string FallBackTypeName { get; set; }
        
        /// <summary>
        /// 故障转移次数
        /// </summary>
        public int FailoverCount { get; set; } = 0;
    }
}