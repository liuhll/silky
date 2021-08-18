using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Gateway;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService
{
    public class SilkyAppService : ISilkyAppService
    {
        private readonly ServiceEntryCache _serviceEntryCache;
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly GatewayCache _gatewayCache;

        public SilkyAppService(ServiceEntryCache serviceEntryCache,
            ServiceRouteCache serviceRouteCache, GatewayCache gatewayCache)
        {
            _serviceEntryCache = serviceEntryCache;
            _serviceRouteCache = serviceRouteCache;
            _gatewayCache = gatewayCache;
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

        public async Task<IReadOnlyCollection<GetGatewayOutput>> GetGateways()
        {
            return _gatewayCache.Gateways.Select(p => new GetGatewayOutput()
            {
                HostName = p.HostName,
                InstanceCount = p.Addresses.Count(),
                SupportServiceCount = p.SupportServices.Count()

            }).ToImmutableArray();
        }
    }
}