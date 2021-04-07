using System;

namespace Silky.Lms.Rpc.Runtime.Client
{
    public class ServiceInvokeInfo
    {
        public int ConcurrentRequests { get; set; } = 0;

        public DateTime FirstInvokeTime { get; set; } = DateTime.Now;

        public DateTime FinalInvokeTime { get; set; } = DateTime.Now;

        public double? AET { get; set; }

        public DateTime? FinalFaultInvokeTime { get; set; }

        public int FaultRequests { get; set; } = 0;

        public int TotalRequests { get; set; } = 0;
    }
}