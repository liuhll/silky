using System.Collections.Generic;
using System.Linq;
using Silky.Core;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Gateway;
using Silky.Rpc.Gateway.Descriptor;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.Descriptor;
using Silky.Rpc.Utils;

namespace Silky.Http.Dashboard.AppService
{
    public class SilkyAppService : ISilkyAppService
    {
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly GatewayCache _gatewayCache;
        private readonly IServiceEntryManager _serviceEntryManager;

        public SilkyAppService(
            ServiceRouteCache serviceRouteCache,
            GatewayCache gatewayCache,
            IServiceEntryManager serviceEntryManager)
        {
            _serviceRouteCache = serviceRouteCache;
            _gatewayCache = gatewayCache;
            _serviceEntryManager = serviceEntryManager;
        }

        public PagedList<GetHostOutput> GetHosts(PagedRequestDto input)
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
            return hosts.ToPagedList(input.PageIndex, input.PageSize);
        }

        public GetDetailHostOutput GetHostDetail(string hostName)
        {
            var detailHostOutput = new GetDetailHostOutput()
            {
                HostName = hostName
            };
            var allServiceEntries = _serviceEntryManager.GetAllEntries();
            var appServices = _serviceRouteCache.ServiceRoutes.Where(p => p.ServiceDescriptor.HostName == hostName)
                .GroupBy(p => (p.ServiceDescriptor.AppService, p.ServiceDescriptor.ServiceProtocol));
            detailHostOutput.AppServices = appServices.Select(p => new HostAppServiceOutput()
            {
                ServiceProtocol = p.Key.ServiceProtocol,
                AppService = p.Key.AppService,
                WsPath = p.Key.ServiceProtocol == ServiceProtocol.Ws ? p.First().ServiceDescriptor.GetWsPath() : null,
                ServiceEntries = allServiceEntries.Where(se =>
                        se.ServiceDescriptor.AppService == p.Key.AppService &&
                        se.ServiceDescriptor.ServiceProtocol == p.Key.ServiceProtocol)
                    .Select(se => new ServiceEntryOutput()
                    {
                        ServiceId = se.Id,
                        MultipleServiceKey = se.MultipleServiceKey,
                        Author = se.ServiceDescriptor.GetAuthor(),
                        WebApi = se.GovernanceOptions.ProhibitExtranet ? null : se.Router.RoutePath,
                        HttpMethod = se.GovernanceOptions.ProhibitExtranet ? null : se.Router.HttpMethod,
                        ProhibitExtranet = se.GovernanceOptions.ProhibitExtranet,
                        Method = se.MethodInfo.Name
                    }).ToArray()
            }).ToArray();
            return detailHostOutput;
        }

        public PagedList<GetHostInstanceOutput> GetHostInstances(GetHostInstanceInput input)
        {
            var hostAddresses = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.HostName == input.HostName &&
                                p.ServiceDescriptor.ServiceProtocol == input.ServiceProtocol)
                    .SelectMany(p => p.Addresses)
                    .Distinct()
                ;
            var hostInstances = new List<GetHostInstanceOutput>();
            foreach (var address in hostAddresses)
            {
                var hostInstance = new GetHostInstanceOutput()
                {
                    HostName = input.HostName,
                    Address = address.IPEndPoint.ToString(),
                    IsEnable = address.Enabled,
                    IsHealth = SocketCheck.TestConnection(address.IPEndPoint),
                    ServiceProtocol = address.ServiceProtocol
                };
                hostInstances.Add(hostInstance);
            }

            return hostInstances.ToPagedList(input.PageIndex, input.PageSize);
        }

        public GetGatewayOutput GetGateway()
        {
            var gateway = _gatewayCache.Gateways.First(p => p.HostName == EngineContext.Current.HostName);
            var gatewayOutput = new GetGatewayOutput()
            {
                HostName = gateway.HostName,
                InstanceCount = gateway.Addresses.Count(),
                SupportServiceCount = gateway.SupportServices.Count()
            };
            return gatewayOutput;
        }

        public PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input)
        {
            var gateway = _gatewayCache.Gateways.First(p => p.HostName == EngineContext.Current.HostName);
            var gatewayInstances = new Dictionary<string, GetGatewayInstanceOutput>();

            foreach (var addressDescriptor in gateway.Addresses)
            {
                var gatewayInstance =
                    gatewayInstances.GetValueOrDefault($"{addressDescriptor.Address}:{addressDescriptor.Port}") ??
                    new GetGatewayInstanceOutput()
                    {
                        HostName = gateway.HostName,
                        Endpoint = $"{addressDescriptor.Address}:{addressDescriptor.Port}",
                        Addresses = new List<AddressOutput>()
                    };
                var address = new AddressOutput()
                {
                    Address = addressDescriptor.ConvertToAddress(),
                    ServiceProtocol = addressDescriptor.ServiceProtocol
                };
                if (!gatewayInstance.Addresses.Contains(address))
                {
                    gatewayInstance.Addresses.Add(address);
                }

                gatewayInstances[$"{addressDescriptor.Address}:{addressDescriptor.Port}"] = gatewayInstance;
            }

            return gatewayInstances.Values.ToPagedList(input.PageIndex, input.PageSize);
        }
    }
}