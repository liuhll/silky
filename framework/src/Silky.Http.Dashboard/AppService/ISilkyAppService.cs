using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Silky.Http.Dashboard.AppService
{
    [ServiceRoute]
    [SilkyAppService]
    [DashboardAppService]
    [Authorize]
    public interface ISilkyAppService
    {
        [Authorize(PermissionCode.GetHosts)]
        PagedList<GetHostOutput> GetHosts(PagedRequestDto input);

        [Authorize(PermissionCode.GetAllHosts)]
        IReadOnlyCollection<GetHostOutput> GetAllHosts();

        [HttpGet("host/{hostName}/detail")]
        [Authorize(PermissionCode.GetHostDetail)]
        ServerDescriptor GetHostDetail(string hostName);


        [HttpGet("host/{hostName}/instances")]
        [Authorize(PermissionCode.GetHostInstances)]
        PagedList<GetHostInstanceOutput> GetHostInstances(string hostName, GetHostInstanceInput input);

        [HttpGet("host/{hostName}/wsservices")]
        [Authorize(PermissionCode.GetWebSocketServices)]
        PagedList<GetWebSocketServiceOutput> GetWebSocketServices(string hostName, PagedRequestDto input);

        [HttpGet("services")]
        [Authorize(PermissionCode.GetServices)]
        IReadOnlyCollection<GetServiceOutput> GetServices(string hostName);

        [Authorize(PermissionCode.GetGateway)]
        GetGatewayOutput GetGateway();

        [HttpGet("gateway/instances")]
        [Authorize(PermissionCode.GetGatewayInstances)]
        PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input);

        [Authorize(PermissionCode.GetGatewayServiceEntries)]
        [HttpGet("gateway/serviceentries")]
        PagedList<ServiceEntryDescriptor> GetGatewayServiceEntries(GetGatewayServiceEntryInput input);

        [Authorize(PermissionCode.GetGatewayServices)]
        [HttpGet("gateway/services")]
        ICollection<GetServiceOutput> GetGatewayServices();

        [Authorize(PermissionCode.GetServiceEntries)]
        PagedList<GetServiceEntryOutput> GetServiceEntries(GetServiceEntryInput input);

        [HttpGet("serviceentry/{serviceEntryId}/detail")]
        [Authorize(PermissionCode.GetServiceEntryDetail)]
        GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceEntryId);

        [Authorize(PermissionCode.GetServiceEntryInstances)]
        [HttpGet("serviceentry/{serviceEntryId}/instances")]
        PagedList<GetServiceEntryInstanceOutput> GetServiceEntryInstances(string serviceEntryId, int pageIndex = 1,
            int pageSize = 10);

        [Authorize(PermissionCode.GetInstanceDetail)]
        [HttpGet("instance/{address}/detail")]
        Task<ServerInstanceDetailInfo> GetInstanceDetail(string address);

        [Authorize(PermissionCode.GetServiceEntryHandleInfos)]
        [HttpGet("instance/{address}/servicehandle")]
        Task<PagedList<ServerHandleInfo>> GetServiceEntryHandleInfos(string address,
            GetServerHandlePagedRequestDto input);

        [Authorize(PermissionCode.GetServiceEntryInvokeInfos)]
        [HttpGet("instance/{address}/serviceinvoke")]
        Task<PagedList<ClientInvokeInfo>> GetServiceEntryInvokeInfos(string address,
            GetClientInvokePagedRequestDto input);

        [Authorize(PermissionCode.GetRegistryCenters)]
        IReadOnlyCollection<GetRegistryCenterOutput> GetRegistryCenters();

        [Authorize(PermissionCode.GetProfiles)]
        IReadOnlyCollection<GetProfileOutput> GetProfiles();

        [Authorize(PermissionCode.GetExternalRoutes)]
        IReadOnlyCollection<GetExternalRouteOutput> GetExternalRoutes();
    }
}