using System;
using Silky.Core.Runtime.Session;

namespace Silky.Rpc.Runtime.Client
{
    [IgnoreMultiTenancy]
    public class ServerInstanceInvokeInfo
    {
        public DateTime? FirstInvokeTime { get; set; }

        public DateTime? FinalInvokeTime { get; set; }

        public double? AET { get; set; }

        public DateTime? FinalFaultInvokeTime { get; set; }

        public int FaultInvokeCount { get; set; } = 0;

        public int TotalInvokeCount { get; set; } = 0;

        public int MaxConcurrentCount { get; set; } = 0;

        public int ConcurrentCount { get; set; } = 0;
    }
}