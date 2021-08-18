using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;

namespace Silky.Http.Dashboard.AppService
{
    [ServiceRoute]
    public interface ISilkyAppService
    {
        [HttpGet("hosts")]
        Task<IReadOnlyCollection<GetHostOutput>> GetHosts();

        [HttpGet("gateways")]
        Task<IReadOnlyCollection<GetGatewayOutput>> GetGateways();
    }
}