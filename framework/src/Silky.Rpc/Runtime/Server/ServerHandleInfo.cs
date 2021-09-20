using System;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerHandleInfo
    {
        public int ConcurrentHandleCount { get; set; } = 0;

        public DateTime FirstHandleTime { get; set; } = DateTime.Now;

        public DateTime FinalHandleTime { get; set; } = DateTime.Now;

        public double? AET { get; set; }

        public DateTime? FinalFaultHandleTime { get; set; }
        public int FaultHandleCount { get; set; } = 0;
        public int SeriousError { get; set; } = 0;
        public int TotalHandleCount { get; set; } = 0;

        public DateTime? SeriousErrorTime { get; set; }
    }
}