using System;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Diagnostics
{
    public class RpcExceptionEventData
    {
        public string MessageId { get; set; }
        
        public string ServiceEntryId { get; set; }

        public StatusCode StatusCode { get; set; }

        public Exception Exception { get; set; }

        public long? ElapsedTimeMs { get; set; }
        public string RemoteAddress { get; set; }
    }
}