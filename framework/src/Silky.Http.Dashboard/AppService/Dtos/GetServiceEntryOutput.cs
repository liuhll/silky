using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryOutput
    {
        public string Application { get; set; }
        public string AppService { get; set; }

        public string ServiceId { get; set; }

        public bool IsEnable { get; set; }

        public int ServiceRouteCount { get; set; }

        public string Method { get; set; }

        public string WebApi { get; set; }

        public HttpMethod? HttpMethod { get; set; }

        public bool MultipleServiceKey { get; set; }

        public bool ProhibitExtranet { get; set; }

        public string Author { get; set; }

        public bool IsDistributeTransaction { get; set; }
    }
}