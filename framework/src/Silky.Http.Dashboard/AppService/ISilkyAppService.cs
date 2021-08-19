using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;

namespace Silky.Http.Dashboard.AppService
{
    [ServiceRoute]
    public interface ISilkyAppService
    {
        [HttpGet("hosts")]
        PagedList<GetHostOutput> GetHosts(PagedRequestDto input);

        [HttpGet("host/instances")]
        PagedList<GetHostInstanceOutput> GetHostInstance(GetHostInstanceInput input);

        [HttpGet("gateways")]
        IReadOnlyCollection<GetGatewayOutput> GetGateways();
    }
}