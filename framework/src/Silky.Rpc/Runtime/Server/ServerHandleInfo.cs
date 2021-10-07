using System;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerHandleInfo
    {
        public DateTime? FirstHandleTime { get; set; } = DateTime.Now;

        public DateTime FinalHandleTime { get; set; } = DateTime.Now;

        public double? AET { get; set; }

        public DateTime? FinalSeriousErrorTime { get; set; }
        public int FaultHandleCount { get; set; } = 0;
        public int SeriousErrorCount { get; set; } = 0;
        public int TotalHandleCount { get; set; } = 0;

        public DateTime? SeriousErrorTime { get; set; }
        public string ServiceEntryId { get; set; }
        public string Address { get; set; }
    }
}