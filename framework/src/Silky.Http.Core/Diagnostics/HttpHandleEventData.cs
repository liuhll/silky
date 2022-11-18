using Microsoft.AspNetCore.Http;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Diagnostics
{
    public class HttpHandleEventData
    {
        public string MessageId { get; set; }

        public HttpContext HttpContext { get; set; }

        public string ServiceEntryId { get; set; }

        public bool IsLocal { get;set; }

        public object[] Parameters { get; set; }

        public long? OperationTimestamp { get; set; }
    }
}