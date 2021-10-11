using Silky.Core.Exceptions;

namespace Silky.Rpc.Diagnostics
{
    public class RpcInvokeResultEventData
    {
        public string MessageId { get; set; }
        public string ServiceEntryId { get; set; }

        public object Result { get; set; }

        public StatusCode StatusCode { get; set; }

        public long? ElapsedTimeMs { get; set; }
    }
}