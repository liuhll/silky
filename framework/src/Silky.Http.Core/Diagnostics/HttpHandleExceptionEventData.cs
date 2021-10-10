using System;
using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Diagnostics
{
    public class HttpHandleExceptionEventData
    {
        public string MessageId { get; set; }

        public ServiceEntry ServiceEntry { get; set; }

        public StatusCode StatusCode { get; set; }

        public Exception Exception { get; set; }

        public long ElapsedTimeMs { get; set; }

        public HttpContext HttpContext { get; set; }
    }
}