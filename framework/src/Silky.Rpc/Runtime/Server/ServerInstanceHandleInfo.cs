using System;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerInstanceHandleInfo
    {
        public DateTime? FirstHandleTime { get; set; }

        public DateTime? FinalHandleTime { get; set; }

        public double? AET { get; set; }

        public DateTime? FinalFaultHandleTime { get; set; }
        public int FaultHandleCount { get; set; } = 0;
        public int TotalSeriousErrorCount { get; set; } = 0;
        public int TotalHandleCount { get; set; } = 0;

        public DateTime? FinalSeriousErrorTime { get; set; }
        public int ConcurrentCount { get; set; } = 0;
        public int MaxConcurrentCount { get; set; } = 0;

        public int AllowMaxConcurrentCount { get; set; }
    }
}