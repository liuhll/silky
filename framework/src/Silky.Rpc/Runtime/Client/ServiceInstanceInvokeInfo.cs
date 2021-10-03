using System;

namespace Silky.Rpc.Runtime.Client
{
    public class ServiceInstanceInvokeInfo
    {
        public DateTime? FirstInvokeTime { get; set; }

        public DateTime? FinalInvokeTime { get; set; }

        public double? AET { get; set; }

        public DateTime? FinalFaultInvokeTime { get; set; }

        public int FaultInvokeCount { get; set; } = 0;

        public int TotalInvokeCount { get; set; } = 0;
    }
}