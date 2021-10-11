using System.Collections.Generic;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core.Rpc;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryOutput
    {
        public string HostName { get; set; }

        public string ServiceName { get; set; }
        public string ServiceId { get; set; }

        public string ServiceEntryId { get; set; }

        public bool IsEnable { get; set; }

        public int ServerInstanceCount { get; set; }

        public string Method { get; set; }

        public string WebApi { get; set; }

        public HttpMethod? HttpMethod { get; set; }

        public bool MultipleServiceKey { get; set; }

        public bool ProhibitExtranet { get; set; }

        public string Author { get; set; }

        public bool IsAllowAnonymous { get; set; }
        public bool IsDistributeTransaction { get; set; }

        public ServiceProtocol ServiceProtocol { get; set; }

        public ICollection<ServiceKeyOutput> ServiceKeys { get; set; }

        public bool IsSystem { get; set; }
    }
}