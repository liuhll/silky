using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Diagnostics
{
    public class HttpHandleResultEventData
    {
        public string MessageId { get; set; }
        
        public object Result { get; set; }
        
        public string ServiceEntryId { get; set; }
        
        public bool IsLocal { get; set; }

        public long ElapsedTimeMs { get; set; }

        public HttpContext HttpContext { get; set; }
        public StatusCode StatusCode { get; } = StatusCode.Success;
    }
}