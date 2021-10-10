using System;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Diagnostics
{
    public class FallbackExceptionEventData
    {
        public string ServiceEntryId { get; set; }
        public Exception Exception { get; set; }
        public string MessageId { get; set; }
        public StatusCode StatusCode { get; set; }
        public IFallbackProvider FallbackProvider { get; set; }
        public long ElapsedTimeMs { get; set; }
    }
}