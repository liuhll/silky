using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Rpc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.AppServices;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.Configuration;
using Silky.Rpc.Gateway;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
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
        private readonly ServiceEntryCache _serviceEntryCache;
        private readonly IRemoteServiceExecutor _serviceExecutor;
        private readonly IRpcAppService _rpcAppService;
        private readonly IRegisterCenterHealthProvider _registerCenterHealthProvider;
        private readonly RegistryCenterOptions _registryCenterOptions;


        private const string ipEndpointRegex =
            @"([0-9]|[1-9]\d{1,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])";

        private const string getInstanceSupervisorServiceId =
            "Silky.Rpc.AppServices.IRpcAppService.GetInstanceSupervisor";

        private const string getGetServiceEntrySupervisorServiceId =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntrySupervisor.serviceId";

        public SilkyAppService(
            ServiceRouteCache serviceRouteCache,
            GatewayCache gatewayCache,
            IServiceEntryManager serviceEntryManager,
            ServiceEntryCache serviceEntryCache,
            IRemoteServiceExecutor serviceExecutor,
            IRpcAppService rpcAppService,
            IRegisterCenterHealthProvider registerCenterHealthProvider,
            IOptions<RegistryCenterOptions> registryCenterOptions)
        {
            _serviceRouteCache = serviceRouteCache;
            _gatewayCache = gatewayCache;
            _serviceEntryManager = serviceEntryManager;
            _serviceEntryCache = serviceEntryCache;
            _serviceExecutor = serviceExecutor;
            _rpcAppService = rpcAppService;
            _registerCenterHealthProvider = registerCenterHealthProvider;
            _registryCenterOptions = registryCenterOptions.Value;
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
            detailHostOutput.AppServices = appServices.Where(p => p.Key.ServiceProtocol == ServiceProtocol.Tcp).Select(
                p => new HostAppServiceOutput()
                {
                    ServiceProtocol = p.Key.ServiceProtocol,
                    AppService = p.Key.AppService,

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
            detailHostOutput.WsServices = appServices.Where(p => p.Key.ServiceProtocol == ServiceProtocol.Ws).Select(
                p => new WsAppServiceOutput()
                {
                    AppService = p.Key.AppService,
                    WsPath = p.First().ServiceDescriptor.GetWsPath(),
                    ServiceProtocol = p.Key.ServiceProtocol
                }).ToArray();
            return detailHostOutput;
        }

        public PagedList<GetHostInstanceOutput> GetHostInstances(string hostName, GetHostInstanceInput input)
        {
            var hostAddresses = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.HostName == hostName &&
                                p.ServiceDescriptor.ServiceProtocol == input.ServiceProtocol)
                    .SelectMany(p => p.Addresses)
                    .Distinct()
                ;
            var hostInstances = new List<GetHostInstanceOutput>();
            foreach (var address in hostAddresses)
            {
                var hostInstance = new GetHostInstanceOutput()
                {
                    HostName = hostName,
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
                InstanceCount = gateway.Addresses.Select(p => new { p.Address, p.Port }).Distinct().Count(),
                SupportServiceCount = gateway.SupportServices.Count(),
                SupportServiceEntryCount = _serviceEntryManager.GetAllEntries().Count
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

        public PagedList<GetServiceEntryOutput> GetServiceEntries(GetServiceEntryInput input)
        {
            var serviceEntryOutputs = _serviceEntryManager.GetAllEntries()
                .Select(p =>
                {
                    var serviceRoute =
                        _serviceRouteCache.ServiceRoutes.FirstOrDefault(sr => sr.ServiceDescriptor.Id == p.Id);
                    var serviceEntryOutput = new GetServiceEntryOutput()
                    {
                        ServiceId = p.Id,
                        Author = p.ServiceDescriptor.GetAuthor(),
                        AppService = p.ServiceDescriptor.AppService,
                        Host = serviceRoute?.ServiceDescriptor.HostName,
                        WebApi = p.GovernanceOptions.ProhibitExtranet ? "" : p.Router.RoutePath,
                        HttpMethod = p.GovernanceOptions.ProhibitExtranet ? null : p.Router.HttpMethod,
                        ProhibitExtranet = p.GovernanceOptions.ProhibitExtranet,
                        Method = p.MethodInfo.Name,
                        MultipleServiceKey = p.MultipleServiceKey,
                        IsOnline = serviceRoute != null,
                        ServiceRouteCount = serviceRoute?.Addresses.Length ?? 0,
                        IsDistributeTransaction = p.IsTransactionServiceEntry()
                    };
                    return serviceEntryOutput;
                }).WhereIf(!input.Host.IsNullOrEmpty(), p => input.Host.Equals(p.Host))
                .WhereIf(!input.AppService.IsNullOrEmpty(), p => input.AppService.Equals(p.AppService))
                .WhereIf(!input.Name.IsNullOrEmpty(), p => p.ServiceId.Contains(input.Name))
                .WhereIf(input.IsOnline.HasValue, p => p.IsOnline == input.IsOnline);

            return serviceEntryOutputs.ToPagedList(input.PageIndex, input.PageSize);
        }

        public GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceId)
        {
            var serviceEntry = _serviceEntryManager.GetAllEntries().FirstOrDefault(p => p.Id == serviceId);
            if (serviceEntry == null)
            {
                throw new BusinessException($"No service entry for {serviceId} exists");
            }

            var serviceRoute =
                _serviceRouteCache.ServiceRoutes.FirstOrDefault(sr => sr.ServiceDescriptor.Id == serviceId);
            var serviceEntryOutput = new GetServiceEntryDetailOutput()
            {
                ServiceId = serviceEntry.Id,
                Author = serviceEntry.ServiceDescriptor.GetAuthor(),
                AppService = serviceEntry.ServiceDescriptor.AppService,
                Host = serviceRoute?.ServiceDescriptor.HostName,
                WebApi = serviceEntry.GovernanceOptions.ProhibitExtranet ? "" : serviceEntry.Router.RoutePath,
                HttpMethod = serviceEntry.GovernanceOptions.ProhibitExtranet ? null : serviceEntry.Router.HttpMethod,
                ProhibitExtranet = serviceEntry.GovernanceOptions.ProhibitExtranet,
                Method = serviceEntry.MethodInfo.Name,
                MultipleServiceKey = serviceEntry.MultipleServiceKey,
                IsOnline = serviceRoute != null,
                ServiceRouteCount = serviceRoute?.Addresses.Length ?? 0,
                GovernanceOptions = serviceEntry.GovernanceOptions,
                IsDistributeTransaction = serviceEntry.IsTransactionServiceEntry()
            };

            return serviceEntryOutput;
        }

        public PagedList<GetServiceEntryRouteOutput> GetServiceEntryRoutes(string serviceId, int pageIndex = 1,
            int pageSize = 10)
        {
            var serviceEntry = _serviceEntryManager.GetAllEntries().FirstOrDefault(p => p.Id == serviceId);
            if (serviceEntry == null)
            {
                throw new BusinessException($"No service entry for {serviceId} exists");
            }

            var serviceEntryInstances = new List<GetServiceEntryRouteOutput>();

            var serviceRoute =
                _serviceRouteCache.ServiceRoutes.FirstOrDefault(sr => sr.ServiceDescriptor.Id == serviceId);

            if (serviceRoute != null)
            {
                foreach (var address in serviceRoute.Addresses)
                {
                    var serviceEntryInstance = new GetServiceEntryRouteOutput()
                    {
                        ServiceId = serviceId,
                        Address = address.IPEndPoint.ToString(),
                        IsEnable = address.Enabled,
                        IsHealth = SocketCheck.TestConnection(address.IPEndPoint),
                        ServiceProtocol = address.ServiceProtocol
                    };
                    serviceEntryInstances.Add(serviceEntryInstance);
                }
            }
            else
            {
                var gateway = _gatewayCache.Gateways.First(p => p.HostName == EngineContext.Current.HostName);
                if (gateway.SupportServices.Any(p => serviceId.Contains(p)))
                {
                    foreach (var addressDescriptor in gateway.Addresses)
                    {
                        var address = addressDescriptor.ConvertToAddressModel();
                        var serviceEntryInstance = new GetServiceEntryRouteOutput()
                        {
                            ServiceId = serviceId,
                            Address = address.IPEndPoint.ToString(),
                            IsEnable = address.Enabled,
                            IsHealth = SocketCheck.TestConnection(address.Address, address.Port),
                            ServiceProtocol = address.ServiceProtocol
                        };
                        if (serviceEntryInstances.All(p => p.Address != serviceEntryInstance.Address))
                        {
                            serviceEntryInstances.AddIfNotContains(serviceEntryInstance);
                        }
                    }
                }
            }

            return serviceEntryInstances.ToPagedList(pageIndex, pageSize);
        }

        public async Task<GetInstanceSupervisorOutput> GetInstanceDetail(string address, bool isGateway = false)
        {
            if (!Regex.IsMatch(address, ipEndpointRegex))
            {
                throw new BusinessException($"{address} incorrect address format");
            }

            var addressInfo = address.Split(":");
            if (!SocketCheck.TestConnection(addressInfo[0], int.Parse(addressInfo[1])))
            {
                throw new BusinessException($"{address} is unHealth");
            }

            if (isGateway)
            {
                return _rpcAppService.GetInstanceSupervisor();
            }

            RpcContext.Context.SetAttachment(AttachmentKeys.ServerAddress, address);

            if (!_serviceEntryCache.TryGetServiceEntry(getInstanceSupervisorServiceId, out var serviceEntry))
            {
                throw new BusinessException($"Not find serviceEntry by {getInstanceSupervisorServiceId}");
            }

            var result = await _serviceExecutor.Execute(serviceEntry, Array.Empty<object>(), null);
            return result as GetInstanceSupervisorOutput;
        }

        public async Task<GetServiceEntrySupervisorOutput> GetServiceEntrySupervisor(string address, string serviceId,
            bool isGateway = false)
        {
            if (!Regex.IsMatch(address, ipEndpointRegex))
            {
                throw new BusinessException($"{address} incorrect address format");
            }

            var addressInfo = address.Split(":");
            if (!SocketCheck.TestConnection(addressInfo[0], int.Parse(addressInfo[1])))
            {
                throw new BusinessException($"{address} is unHealth");
            }

            if (isGateway)
            {
                return _rpcAppService.GetServiceEntrySupervisor(serviceId);
            }

            RpcContext.Context.SetAttachment(AttachmentKeys.ServerAddress, address);
            if (!_serviceEntryCache.TryGetServiceEntry(getGetServiceEntrySupervisorServiceId, out var serviceEntry))
            {
                throw new BusinessException($"Not find serviceEntry by {getInstanceSupervisorServiceId}");
            }

            var result = await _serviceExecutor.Execute(serviceEntry, new object[1] { serviceId }, null);
            return result as GetServiceEntrySupervisorOutput;
        }

        public IReadOnlyCollection<GetRegistryCenterOutput> GetRegistryCenters()
        {
            var registerCenterInfos = _registerCenterHealthProvider.GetRegistryCenterHealthInfo();
            return registerCenterInfos.Select(p =>
                new GetRegistryCenterOutput()
                {
                    RegistryCenterAddress = p.Key,
                    IsHealth = p.Value.IsHealth,
                    UnHealthReason = p.Value.UnHealthReason,
                    UnHealthTimes = p.Value.UnHealthTimes,
                    RegistryCenterType = _registryCenterOptions.RegistryCenterType
                }).ToArray();
        }

        public IReadOnlyCollection<GetProfileOutput> GetProfiles()
        {
            var getProfileOutputs = new List<GetProfileOutput>();
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "Microservice",
                Title = "微服务应用",
                Count = _serviceRouteCache.ServiceRoutes.GroupBy(p => p.ServiceDescriptor.HostName).Select(p => p.Key)
                    .Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceInstance",
                Title = "微服务主机实例",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Tcp)
                    .SelectMany(p => p.Addresses)
                    .Select(p => new { p.Address, p.Port }).Distinct().Count()
            });

            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceInstance",
                Title = "Ws微服务主机实例",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Ws)
                    .SelectMany(p => p.Addresses)
                    .Select(p => new { p.Address, p.Port }).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "AppService",
                Title = "应用服务",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Tcp)
                    .Select(p => p.ServiceDescriptor.AppService).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WsService",
                Title = "Websocket服务",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Ws)
                    .Select(p => p.ServiceDescriptor.Id).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceEntry",
                Title = "服务条目",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Tcp)
                    .Select(p => p.ServiceDescriptor.Id).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "RegistryCenter",
                Title = "服务注册中心",
                Count = _registerCenterHealthProvider.GetRegistryCenterHealthInfo().Count
            });

            return getProfileOutputs.Where(p => p.Count > 0).ToArray();
        }
    }
}