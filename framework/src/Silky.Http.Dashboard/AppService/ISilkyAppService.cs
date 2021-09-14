using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using GetInstanceDetailOutput = Silky.Rpc.AppServices.Dtos.GetInstanceDetailOutput;

namespace Silky.Http.Dashboard.AppService
{
    [ServiceRoute(Application = "Dashboard")]
    public interface ISilkyAppService
    {
        PagedList<GetApplicationOutput> GetApplications(PagedRequestDto input);

        IReadOnlyCollection<GetApplicationOutput> GetAllApplications();

        [HttpGet("application/{appName:string}/detail")]
        GetDetailApplicationOutput GetApplicationDetail(string appName);

        IReadOnlyCollection<GetServiceOutput> GetServices(string appName);

        [HttpGet("application/{appName:string}/instances")]
        PagedList<GetApplicationInstanceOutput> GetApplicationInstances(string appName,
            GetApplicationInstanceInput input);

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