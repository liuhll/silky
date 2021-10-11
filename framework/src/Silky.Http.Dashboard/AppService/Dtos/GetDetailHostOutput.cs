using System.Collections.Generic;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core.Rpc;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetDetailHostOutput
    {
        public GetDetailHostOutput()
        {
            AppServiceEntries = new List<ServiceEntryOutput>();
            WsServices = new List<WsAppServiceOutput>();
        }

        public string HostName { get; set; }

        public IReadOnlyCollection<ServiceEntryOutput> AppServiceEntries { get; set; }

        public IReadOnlyCollection<WsAppServiceOutput> WsServices { get; set; }
    }


    public class WsAppServiceOutput
    {
        public string AppService { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }

        public string WsPath { get; set; }
    }

    public class ServiceEntryOutput
    {
        public ServiceProtocol ServiceProtocol { get; set; }

        public string HostName { get; set; }

        public string Application { get; set; }
        public string ServiceName { get; set; }
        public string ServiceId { get; set; }

        public string ServiceEntryId { get; set; }

        public string Method { get; set; }

        public string WebApi { get; set; }

        public HttpMethod? HttpMethod { get; set; }

        public bool MultipleServiceKey { get; set; }

        public bool ProhibitExtranet { get; set; }

        public string Author { get; set; }
    }
}