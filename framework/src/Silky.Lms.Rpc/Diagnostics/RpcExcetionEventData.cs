using System;
using Silky.Lms.Core.Exceptions;

namespace Silky.Lms.Rpc.Diagnostics
{
    public class RpcExcetionEventData
    {
        public string MessageId { get; set; }
        public string ServiceId { get; set; }

        public StatusCode StatusCode { get; set; }

        public Exception Exception { get; set; }

        public long? ElapsedTimeMs { get; set; }
        public string RemoteAddress { get; set; }
    }
}