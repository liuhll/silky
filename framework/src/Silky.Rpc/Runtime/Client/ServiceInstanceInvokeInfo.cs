using System;

namespace Silky.Rpc.Runtime.Client
{
    public class ServiceInstanceInvokeInfo
    {
        public int MaxConcurrentRequests { get; set; } = 0;
        public DateTime? FirstInvokeTime { get; set; }

        public DateTime? FinalInvokeTime { get; set; }

        public double? AET { get; set; }

        public DateTime? FinalFaultInvokeTime { get; set; }

        public int FaultRequests { get; set; } = 0;

        public int TotalRequests { get; set; } = 0;
    }
}