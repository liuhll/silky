using Silky.Rpc.Messages;

namespace Silky.Rpc.Diagnostics
{
    public class RpcInvokeEventData
    {
        public string MessageId { get; set; }
        public long? OperationTimestamp { get; set; }
        public string ServiceId { get; set; }
        public bool IsGateWay { get; set; }

        public RemoteInvokeMessage Message { get; set; }

        public string RemoteAddress { get; set; }
        public string ServiceMethodName { get; set; }
    }
}