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
    [Authorize(PermissionCode.Default)]
    public interface ISilkyAppService
    {
        PagedList<GetHostOutput> GetHosts(PagedRequestDto input);
        
        IReadOnlyCollection<GetHostOutput> GetAllHosts();

        [HttpGet("host/{hostName}/detail")]
        ServerDescriptor GetHostDetail(string hostName);
        
        [HttpGet("host/{hostName}/instances")]
        Task<PagedList<GetHostInstanceOutput>> GetHostInstances(string hostName, GetHostInstanceInput input);

        [HttpGet("host/{hostName}/wsservices")]
        PagedList<GetWebSocketServiceOutput> GetWebSocketServices(string hostName, PagedRequestDto input);

        [HttpGet("services")]
        IReadOnlyCollection<GetServiceOutput> GetServices(string hostName);
        
        GetGatewayOutput GetGateway();

        [HttpGet("gateway/instances")]
        PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input);
        
        [HttpGet("gateway/serviceentries")]
        PagedList<ServiceEntryDescriptor> GetGatewayServiceEntries(GetGatewayServiceEntryInput input);
        
        [HttpGet("gateway/services")]
        ICollection<GetServiceOutput> GetGatewayServices();
        
        PagedList<GetServiceEntryOutput> GetServiceEntries(GetServiceEntryInput input);

        [HttpGet("serviceentry/{serviceEntryId}/detail")]
        GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceEntryId);
        
        [HttpGet("serviceentry/{serviceEntryId}/instances")]
        PagedList<GetServiceEntryInstanceOutput> GetServiceEntryInstances(string serviceEntryId, int pageIndex = 1,
            int pageSize = 10);
        
        [HttpGet("instance/{address}/detail")]
        Task<ServerInstanceDetailInfo> GetInstanceDetail(string address);
        
        [HttpGet("instance/{address}/servicehandle")]
        Task<PagedList<ServerHandleInfo>> GetServiceEntryHandleInfos(string address,
            GetServerHandlePagedRequestDto input);
        
        [HttpGet("instance/{address}/serviceinvoke")]
        Task<PagedList<ClientInvokeInfo>> GetServiceEntryInvokeInfos(string address,
            GetClientInvokePagedRequestDto input);
        
        IReadOnlyCollection<GetRegistryCenterOutput> GetRegistryCenters();
        
        IReadOnlyCollection<GetProfileOutput> GetProfiles();
        
        IReadOnlyCollection<GetExternalRouteOutput> GetExternalRoutes();
    }
}