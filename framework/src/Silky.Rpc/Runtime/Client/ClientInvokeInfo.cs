using System;
using Silky.Core.Runtime.Session;

namespace Silky.Rpc.Runtime.Client
{
    [IgnoreMultiTenancy]
    public class ClientInvokeInfo
    {
        public string ServiceEntryId { get; set; }

        public string Address { get; set; }

        public DateTime FirstInvokeTime { get; set; } = DateTime.Now;

        public DateTime FinalInvokeTime { get; set; } = DateTime.Now;

        public double? AET { get; set; }

        public DateTime? FinalFaultInvokeTime { get; set; }

        public bool IsEnable { get; set; }

        public int FaultInvokeCount { get; set; } = 0;

        public int TotalInvokeCount { get; set; } = 0;
    }
}