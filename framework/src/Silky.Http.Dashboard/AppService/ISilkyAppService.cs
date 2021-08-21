using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Runtime.Server.ServiceDiscovery;

namespace Silky.Http.Dashboard.AppService
{
    [ServiceRoute]
    public interface ISilkyAppService
    {
        PagedList<GetHostOutput> GetHosts(PagedRequestDto input);

        [HttpGet("host/{hostName:string}/detail")]
        GetDetailHostOutput GetHostDetail(string hostName);

        [HttpGet("host/instances")]
        PagedList<GetHostInstanceOutput> GetHostInstances(GetHostInstanceInput input);

        GetGatewayOutput GetGateway();

        [HttpGet("gateway/instances")]
        PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input);

        PagedList<GetServiceEntryOutput> GetServiceEntries(GetServiceEntryInput input);

        [HttpGet("serviceentry/{serviceId:string}/detail")]
        GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceId);

        [HttpGet("serviceentry/{serviceId:string}/routes")]
        PagedList<GetServiceEntryRouteOutput> GetServiceEntryRoutes(string serviceId, int pageIndex = 1,
            int pageSize = 10);

        [HttpGet("instance/{address:string}/detail")]
        Task<GetInstanceSupervisorOutput> GetInstanceDetail(string address,bool isGateway = false);
        
    }
}