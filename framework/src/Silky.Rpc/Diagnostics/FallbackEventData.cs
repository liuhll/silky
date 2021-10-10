using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Diagnostics
{
    public class FallbackEventData
    {
        public string ServiceEntryId { get; set; }
        public object[] Parameters { get; set; }
        public string MessageId { get; set; }
        public FallbackExecType FallbackExecType { get; set; }
        public IFallbackProvider FallbackProvider { get; set; }
        public long OperationTimestamp { get; set; }
    }
}