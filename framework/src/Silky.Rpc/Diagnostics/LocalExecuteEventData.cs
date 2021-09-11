using System;
using JetBrains.Annotations;

namespace Silky.Rpc.Diagnostics
{
    public class LocalExecuteEventData
    {
        public string ServiceEntryId { get; set; }
        
        public string[] FilterNames { get; set; }
        
        public string MethodName { get; set; }

        [CanBeNull] public Exception Exception { get; set; }

        public bool IsAsyncMethod { get; set; }
        
        public bool IsDistributeTrans { get; set; }
    }
}