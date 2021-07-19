using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Diagnostics
{
    public class RpcInvokeEventData
    {
        public string MessageId { get; set; }
        public long? OperationTimestamp { get; set; }
        public string Operation { get; set; }

        public string Method { get; set; }

        public RemoteInvokeMessage Message { get; set; }

        public string RemoteAddress { get; set; }
    }
}