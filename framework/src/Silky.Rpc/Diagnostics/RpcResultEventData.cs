using Silky.Core.Exceptions;

namespace Silky.Rpc.Diagnostics
{
    public class RpcResultEventData
    {
        public string MessageId { get; set; }
        public string ServiceEntryId { get; set; }

        public object Result { get; set; }

        public StatusCode StatusCode { get; set; }

        public string RemoteAddress { get; set; }

        public long? ElapsedTimeMs { get; set; }
    }
}