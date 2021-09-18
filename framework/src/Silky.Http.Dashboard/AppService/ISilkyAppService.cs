using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using GetInstanceDetailOutput = Silky.Rpc.AppServices.Dtos.GetInstanceDetailOutput;

namespace Silky.Http.Dashboard.AppService
{
    [ServiceRoute(Application = "Dashboard")]
    [Metadata(ServiceConstant.IsSilkyService,true)]
    public interface ISilkyAppService
    {
        PagedList<GetHostOutput> GetHosts(PagedRequestDto input);

        IReadOnlyCollection<GetHostOutput> GetAllHosts();

        [HttpGet("host/{hostName:string}/detail")]
        GetDetailHostOutput GetHostDetail(string hostName);

        IReadOnlyCollection<GetServiceOutput> GetServices(string hostName);

        [HttpGet("host/{hostName:string}/instances")]
        PagedList<GetHostInstanceOutput> GetHostInstances(string hostName,
            GetHostInstanceInput input);

        GetGatewayOutput GetGateway();

        [HttpGet("gateway/instances")]
        PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input);

        PagedList<GetServiceEntryOutput> GetServiceEntries(GetServiceEntryInput input);

        [HttpGet("serviceentry/{serviceEntryId:string}/detail")]
        GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceEntryId);

        [HttpGet("serviceentry/{serviceEntryId:string}/routes")]
        PagedList<GetServiceEntryRouteOutput> GetServiceEntryRoutes(string serviceEntryId, int pageIndex = 1,
            int pageSize = 10);


        [HttpGet("instance/{address:string}/detail")]
        Task<GetInstanceDetailOutput> GetInstanceDetail(string address);

        [HttpGet("instance/{address:string}/servicehandle")]
        Task<PagedList<ServiceEntryHandleInfo>> GetServiceEntryHandleInfos(string address, PagedRequestDto input);

        [HttpGet("instance/{address:string}/serviceinvoke")]
        Task<PagedList<ServiceEntryInvokeInfo>> GetServiceEntryInvokeInfos(string address, PagedRequestDto input);

        IReadOnlyCollection<GetRegistryCenterOutput> GetRegistryCenters();

        IReadOnlyCollection<GetProfileOutput> GetProfiles();

        IReadOnlyCollection<GetExternalRouteOutput> GetExternalRoutes();
    }
}