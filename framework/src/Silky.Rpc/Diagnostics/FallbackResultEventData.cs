using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Diagnostics
{
    public class FallbackResultEventData
    {
        public string ServiceEntryId { get; set; }
        public object Result { get; set; }
        public string MessageId { get; set; }
        public IFallbackProvider FallbackProvider { get; set; }
        public long ElapsedTimeMs { get; set; }
    }
}