using System;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceInstanceHandleInfo
    {
        public int MaxConcurrentHandles { get; set; } = 0;

        public DateTime? FirstHandleTime { get; set; }

        public DateTime? FinalHandleTime { get; set; }

        public double? AET { get; set; }

        public DateTime? FinalFaultHandleTime { get; set; }
        public int FaultHandles { get; set; } = 0;
        public int TotalSeriousError { get; set; } = 0;
        public int TotalHandles { get; set; } = 0;

        public DateTime? FinalSeriousErrorTime { get; set; }
    }
}