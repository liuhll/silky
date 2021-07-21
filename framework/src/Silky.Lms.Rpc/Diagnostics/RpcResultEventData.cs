using Silky.Lms.Core.Exceptions;

namespace Silky.Lms.Rpc.Diagnostics
{
    public class RpcResultEventData
    {
        public string MessageId { get; set; }
        public string ServiceId { get; set; }

        public object Result { get; set; }

        public StatusCode StatusCode { get; set; }

        public string RemoteAddress { get; set; }

        public long? ElapsedTimeMs { get; set; }
    }
}