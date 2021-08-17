using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;
using Silky.Rpc.Security;

namespace Silky.Http.Dashboard.AppService
{
    [ServiceRoute]
    [AllowAnonymous]
    public interface IDashboardAppService
    {
        [HttpGet("hosts")]
        Task<IReadOnlyCollection<GetHostOutput>> GetHosts();
    }
}