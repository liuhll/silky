using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Silky.Rpc.Runtime.Server
{
    internal class HttpMethodInfo
    {
        public string Template { get; set; }

        public bool IsSpecify { get; set; }

        public HttpMethod HttpMethod { get; set; }
    }
}