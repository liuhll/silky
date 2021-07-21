using Microsoft.AspNetCore.Http;

namespace Silky.Rpc.SkyApm.Diagnostics.Http.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsWebApi(this HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Content-Type"))
            {
                return false;
            }

            var contextType = request.Headers["Content-Type"];
            if ("application/json".Equals(contextType) 
                || "text/json".Equals(contextType)
                || "multipart/form-data".Equals(contextType)
                || "application/x-www-form-urlencoded".Equals(contextType)
                || "text/xml".Equals(contextType))
            {
                return true;
            }

            return false;
        }
    }
}