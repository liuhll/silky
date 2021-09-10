using Silky.Rpc.Messages;

namespace Silky.Rpc.Diagnostics
{
    public class RpcInvokeEventData
    {
        public string MessageId { get; set; }
        public long? OperationTimestamp { get; set; }
        public string ServiceEntryId { get; set; }
        public RemoteInvokeMessage Message { get; set; }
    }
}