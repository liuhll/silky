using System;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Diagnostics
{
    public class RpcInvokeExceptionEventData
    {
        public string MessageId { get; set; }

        public string ServiceEntryId { get; set; }

        public StatusCode StatusCode { get; set; }

        public Exception Exception { get; set; }

        public long? ElapsedTimeMs { get; set; }
        public string ClientAddress { get; set; }
    }
}