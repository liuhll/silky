using System;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerHandleInfo
    {
        public int ConcurrentHandles { get; set; } = 0;

        public DateTime FirstHandleTime { get; set; } = DateTime.Now;

        public DateTime FinalHandleTime { get; set; } = DateTime.Now;

        public double? AET { get; set; }

        public DateTime? FinalFaultHandleTime { get; set; }
        public int FaultHandles { get; set; } = 0;
        public int SeriousError { get; set; } = 0;
        public int TotalHandles { get; set; } = 0;

        public DateTime? SeriousErrorTime { get; set; }
    }
}