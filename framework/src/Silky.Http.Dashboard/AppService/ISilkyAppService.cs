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
    [ServiceRoute]
    [Metadata(ServiceConstant.IsSilkyService, true)]
    public interface ISilkyAppService
    {
        PagedList<GetHostOutput> GetHosts(PagedRequestDto input);

        IReadOnlyCollection<GetHostOutput> GetAllHosts();

        [HttpGet("host/{hostName}/detail")]
        ServerDescriptor GetHostDetail(string hostName);
        

        [HttpGet("host/{hostName}/instances")]
        PagedList<GetHostInstanceOutput> GetHostInstances(string hostName, GetHostInstanceInput input);

        [HttpGet("services")]
        IReadOnlyCollection<GetServiceOutput> GetServices(string hostName);

        GetGatewayOutput GetGateway();

        [HttpGet("gateway/instances")]
        PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input);

        PagedList<GetServiceEntryOutput> GetServiceEntries(GetServiceEntryInput input);

        [HttpGet("serviceentry/{serviceEntryId}/detail")]
        GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceEntryId);

        [HttpGet("serviceentry/{serviceEntryId}/instances")]
        PagedList<GetServiceEntryInstanceOutput> GetServiceEntryInstances(string serviceEntryId, int pageIndex = 1,
            int pageSize = 10);


        [HttpGet("instance/{address}/detail")]
        Task<GetInstanceDetailOutput> GetInstanceDetail(string address);

        [HttpGet("instance/{address}/servicehandle")]
        Task<PagedList<ServiceEntryHandleInfo>> GetServiceEntryHandleInfos(string address, PagedRequestDto input);

        [HttpGet("instance/{address}/serviceinvoke")]
        Task<PagedList<ServiceEntryInvokeInfo>> GetServiceEntryInvokeInfos(string address, PagedRequestDto input);

        IReadOnlyCollection<GetRegistryCenterOutput> GetRegistryCenters();

        IReadOnlyCollection<GetProfileOutput> GetProfiles();

        IReadOnlyCollection<GetExternalRouteOutput> GetExternalRoutes();
    }
}