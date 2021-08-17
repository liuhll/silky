using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService
{
    public class DashboardAppService : IDashboardAppService
    {
        private readonly ServiceEntryCache _serviceEntryCache;
        private readonly ServiceRouteCache _serviceRouteCache;

        public DashboardAppService(ServiceEntryCache serviceEntryCache,
            ServiceRouteCache serviceRouteCache)
        {
            _serviceEntryCache = serviceEntryCache;
            _serviceRouteCache = serviceRouteCache;
        }

        public async Task<IReadOnlyCollection<GetHostOutput>> GetHosts()
        {
            var serviceRoutes = _serviceRouteCache.ServiceRoutes;
            var hosts = serviceRoutes.GroupBy(p => p.ServiceDescriptor.HostName)
                .Select(p => new GetHostOutput()
                {
                    HostName = p.Key,
                    InstanceCount = p.Max(p => p.Addresses.Length),
                    ServiceEntriesCount = p.Count(),
                    AppServiceCount = p.GroupBy(p => p.ServiceDescriptor.AppService).Count(),
                    HasWsService = p.Any(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Ws)
                });
            return hosts.ToArray();
        }
    }
}