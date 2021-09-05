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

        private const string getGetServiceEntrySupervisorServiceHandle =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntryHandleInfos";
        
        private const string getGetServiceEntrySupervisorServiceInvoke =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntryInvokeInfos";

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

        public IReadOnlyCollection<GetApplicationOutput> GetApplications()
        {
            var serviceRoutes = _serviceRouteCache.ServiceRoutes;
            var hosts = serviceRoutes.GroupBy(p => p.ServiceDescriptor.HostName)
                .Where(p => p.Key != typeof(IRpcAppService).Name)
                .Select(p => new GetApplicationOutput()
                {
                    HostName = p.Key,
                    InstanceCount = p.Where(sr =>
                            sr.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Tcp)
                        .Max(sr => sr.Addresses.Length),
                    ServiceEntriesCount = p.Count(),
                    AppServiceCount = p.GroupBy(p => p.ServiceDescriptor.AppService).Count(),
                    HasWsService = p.Any(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Ws)
                });
            return hosts.ToArray();
        }

        public GetDetailApplicationOutput GetApplicationDetail(string appName)
        {
            var detailHostOutput = new GetDetailApplicationOutput()
            {
                HostName = appName
            };
            var allServiceEntries = _serviceEntryManager.GetAllEntries();
            var appServices = _serviceRouteCache.ServiceRoutes.Where(p => p.ServiceDescriptor.HostName == appName)
                .OrderBy(p => p.ServiceDescriptor.AppService);

            detailHostOutput.AppServiceEntries = appServices
                .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Tcp).Select(
                    p =>
                    {
                        var se = allServiceEntries.SingleOrDefault(se => se.Id == p.ServiceDescriptor.Id);
                        if (se != null)
                        {
                            return new ServiceEntryOutput()
                            {
                                ServiceProtocol = p.ServiceDescriptor.ServiceProtocol,
                                AppService = p.ServiceDescriptor.AppService,
                                ServiceId = p.ServiceDescriptor.Id,
                                MultipleServiceKey = se.MultipleServiceKey,
                                Author = se.ServiceDescriptor.GetAuthor(),
                                WebApi = se.GovernanceOptions.ProhibitExtranet ? null : se.Router.RoutePath,
                                HttpMethod = se.GovernanceOptions.ProhibitExtranet ? null : se.Router.HttpMethod,
                                ProhibitExtranet = se.GovernanceOptions.ProhibitExtranet,
                                Method = se.MethodInfo.Name
                            };
                        }

                        return null;
                    }).Where(o => o != null).ToArray();

            detailHostOutput.WsServices = appServices
                .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Ws).Select(
                    p => new WsAppServiceOutput()
                    {
                        AppService = p.ServiceDescriptor.AppService,
                        WsPath = p.ServiceDescriptor.GetWsPath(),
                        ServiceProtocol = p.ServiceDescriptor.ServiceProtocol
                    }).ToArray();
            return detailHostOutput;
        }

        public IReadOnlyCollection<GetServiceOutput> GetServices(string appName)
        {
            var appServiceGroups = _serviceRouteCache.ServiceRoutes
                .WhereIf(!appName.IsNullOrEmpty(), p => p.ServiceDescriptor.HostName == appName)
                .OrderBy(p => p.ServiceDescriptor.AppService).GroupBy(p =>
                    new { p.ServiceDescriptor.HostName, p.ServiceDescriptor.AppService, });
            var services = new List<GetServiceOutput>();
            foreach (var appServiceGroup in appServiceGroups)
            {
                services.Add(new GetServiceOutput()
                {
                    AppService = appServiceGroup.Key.AppService,
                    Application = appServiceGroup.Key.HostName,
                    InstanceCount = _serviceRouteCache.ServiceRoutes.Where(p =>
                        p.ServiceDescriptor.AppService == appServiceGroup.Key.AppService
                        && p.ServiceDescriptor.HostName == appServiceGroup.Key.HostName).Max(p => p.Addresses.Length)
                });
            }

            return services.ToArray();
        }

        public PagedList<GetApplicationInstanceOutput> GetApplicationInstances(string appName,
            GetApplicationInstanceInput input)
        {
            var hostAddresses = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.HostName == appName &&
                                p.ServiceDescriptor.ServiceProtocol == input.ServiceProtocol)
                    .SelectMany(p => p.Addresses)
                    .Distinct()
                ;
            var hostInstances = new List<GetApplicationInstanceOutput>();
            foreach (var address in hostAddresses)
            {
                var hostInstance = new GetApplicationInstanceOutput()
                {
                    HostName = appName,
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
                SupportServiceEntryCount = _serviceEntryManager.GetAllEntries().Count,
                ExistWebSocketService =
                    _serviceRouteCache.ServiceRoutes.Any(p =>
                        p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Ws),
                SupportServices = gateway.SupportServices
            };
            return gatewayOutput;
        }

        public PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input)
        {
            var gateway = _gatewayCache.Gateways.First(p => p.HostName == EngineContext.Current.HostName);

            var gatewayInstances = new List<GetGatewayInstanceOutput>();
            foreach (var addressDescriptor in gateway.Addresses)
            {
                var gatewayInstance = new GetGatewayInstanceOutput()
                {
                    HostName = gateway.HostName,
                    Address = addressDescriptor.ConvertToAddress(),
                };
                gatewayInstances.Add(gatewayInstance);
            }

            return gatewayInstances.ToPagedList(input.PageIndex, input.PageSize);
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
                        Application = serviceRoute?.ServiceDescriptor.HostName,
                        WebApi = p.GovernanceOptions.ProhibitExtranet ? "" : p.Router.RoutePath,
                        HttpMethod = p.GovernanceOptions.ProhibitExtranet ? null : p.Router.HttpMethod,
                        ProhibitExtranet = p.GovernanceOptions.ProhibitExtranet,
                        Method = p.MethodInfo.Name,
                        MultipleServiceKey = p.MultipleServiceKey,
                        IsEnable = serviceRoute != null &&
                                   serviceRoute.Addresses.Any(am => SocketCheck.TestConnection(am.Address, am.Port)),
                        ServiceRouteCount = serviceRoute?.Addresses.Length ?? 0,
                        IsDistributeTransaction = p.IsTransactionServiceEntry()
                    };
                    return serviceEntryOutput;
                }).Where(p => !p.Application.IsNullOrEmpty())
                .WhereIf(!input.Application.IsNullOrEmpty(), p => input.Application.Equals(p.Application))
                .WhereIf(!input.AppService.IsNullOrEmpty(), p => input.AppService.Equals(p.AppService))
                .WhereIf(!input.Name.IsNullOrEmpty(), p => p.ServiceId.Contains(input.Name))
                .WhereIf(input.IsEnable.HasValue, p => p.IsEnable == input.IsEnable)
                .OrderBy(p => p.Application).ThenBy(p => p.AppService);

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
                Application = serviceRoute?.ServiceDescriptor.HostName,
                WebApi = serviceEntry.GovernanceOptions.ProhibitExtranet ? "" : serviceEntry.Router.RoutePath,
                HttpMethod = serviceEntry.GovernanceOptions.ProhibitExtranet ? null : serviceEntry.Router.HttpMethod,
                ProhibitExtranet = serviceEntry.GovernanceOptions.ProhibitExtranet,
                Method = serviceEntry.MethodInfo.Name,
                MultipleServiceKey = serviceEntry.MultipleServiceKey,
                IsEnable = serviceRoute != null &&
                           serviceRoute.Addresses.Any(p => SocketCheck.TestConnection(p.Address, p.Port)),
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

        public async Task<GetInstanceSupervisorOutput> GetInstanceDetail(string address)
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
            
            RpcContext.Context.SetAttachment(AttachmentKeys.SelectedAddress, address);

            if (!_serviceEntryCache.TryGetServiceEntry(getInstanceSupervisorServiceId, out var serviceEntry))
            {
                throw new BusinessException($"Not find serviceEntry by {getInstanceSupervisorServiceId}");
            }

            var result = await _serviceExecutor.Execute(serviceEntry, Array.Empty<object>(), null);
            return result as GetInstanceSupervisorOutput;
        }

        public async Task<PagedList<ServiceEntryHandleInfo>> GetServiceEntryHandleInfos(string address,PagedRequestDto input)
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
            
            RpcContext.Context.SetAttachment(AttachmentKeys.SelectedAddress, address);
            
            if (!_serviceEntryCache.TryGetServiceEntry(getGetServiceEntrySupervisorServiceHandle, out var serviceEntry))
            {
                throw new BusinessException($"Not find serviceEntry by {getGetServiceEntrySupervisorServiceHandle}");
            }
            
            var result = await _serviceExecutor.Execute(serviceEntry, Array.Empty<object>(), null) as IReadOnlyCollection<ServiceEntryHandleInfo>;
            return result.ToPagedList(input.PageIndex, input.PageSize);
        }
        
        
        public async Task<PagedList<ServiceEntryInvokeInfo>> GetServiceEntryInvokeInfos(string address,PagedRequestDto input)
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
            
            RpcContext.Context.SetAttachment(AttachmentKeys.SelectedAddress, address);
            
            if (!_serviceEntryCache.TryGetServiceEntry(getGetServiceEntrySupervisorServiceInvoke, out var serviceEntry))
            {
                throw new BusinessException($"Not find serviceEntry by {getGetServiceEntrySupervisorServiceInvoke}");
            }
            
            var result = await _serviceExecutor.Execute(serviceEntry, Array.Empty<object>(), null) as IReadOnlyCollection<ServiceEntryInvokeInfo>;
            return result.ToPagedList(input.PageIndex, input.PageSize);
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
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => !p.ServiceDescriptor.HostName.IsNullOrEmpty() &&
                                p.ServiceDescriptor.HostName != typeof(IRpcAppService).Name)
                    .GroupBy(p => p.ServiceDescriptor.HostName).Select(p => p.Key)
                    .Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceInstance",
                Title = "微服务应用实例",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Tcp)
                    .SelectMany(p => p.Addresses)
                    .Select(p => new { p.Address, p.Port }).Distinct().Count()
            });

            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketServiceInstance",
                Title = "支持WebSocket的应用实例",
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
                Code = "WebSocketService",
                Title = "WebSocket服务",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.ServiceDescriptor.ServiceProtocol == ServiceProtocol.Ws)
                    .Select(p => p.ServiceDescriptor.Id).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceEntry",
                Title = "服务条目",
                Count = _serviceEntryManager.GetAllEntries()
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