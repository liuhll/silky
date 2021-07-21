using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Diagnostics
{
    public class RpcInvokeEventData
    {
        public string MessageId { get; set; }
        public long? OperationTimestamp { get; set; }
        public string ServiceId { get; set; }
        public bool IsGateWay { get; set; }

        public RemoteInvokeMessage Message { get; set; }

        public string RemoteAddress { get; set; }
    }
}