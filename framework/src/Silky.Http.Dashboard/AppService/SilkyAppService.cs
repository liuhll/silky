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
using Silky.Http.Dashboard.Configuration;
using Silky.Rpc.AppServices;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.CachingInterceptor.Providers;
using Silky.Rpc.Configuration;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Http.Dashboard.AppService
{
    public class SilkyAppService : ISilkyAppService
    {
        private readonly ServerRouteCache _serverRouteCache;
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IRemoteExecutor _remoteExecutor;
        private readonly ILocalExecutor _localExecutor;
        private readonly IRegisterCenterHealthProvider _registerCenterHealthProvider;
        private readonly RegistryCenterOptions _registryCenterOptions;


        private const string ipEndpointRegex =
            @"([0-9]|[1-9]\d{1,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])";

        private const string getInstanceSupervisorServiceEntryId =
            "Silky.Rpc.AppServices.IRpcAppService.GetInstanceDetail_Get";

        private const string getGetServiceEntrySupervisorServiceHandle =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntryHandleInfos_Get";

        private const string getGetServiceEntrySupervisorServiceInvoke =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntryInvokeInfos_Get";

        public SilkyAppService(
            ServerRouteCache serverRouteCache,
            IServiceEntryManager serviceEntryManager,
            IServiceEntryLocator serviceEntryLocator,
            IRemoteExecutor remoteExecutor,
            ILocalExecutor localExecutor,
            IRegisterCenterHealthProvider registerCenterHealthProvider,
            IOptions<RegistryCenterOptions> registryCenterOptions)
        {
            _serverRouteCache = serverRouteCache;
            _serviceEntryManager = serviceEntryManager;

            _remoteExecutor = remoteExecutor;
            _registerCenterHealthProvider = registerCenterHealthProvider;
            _localExecutor = localExecutor;
            _serviceEntryLocator = serviceEntryLocator;
            _registryCenterOptions = registryCenterOptions.Value;
        }

        public PagedList<GetHostOutput> GetHosts(PagedRequestDto input)
        {
            return GetAllHosts().ToPagedList(input.PageIndex, input.PageSize);
        }

        public IReadOnlyCollection<GetHostOutput> GetAllHosts()
        {
            var serviceRoutes = _serverRouteCache.ServiceRoutes;
            var hosts = serviceRoutes
                .Where(p => p.Services.GetHostName() != null)
                .GroupBy(p => p.Services.GetHostName())
                .Select(p => new GetHostOutput()
                {
                    Host = p.Key,
                    ApplicationCount = p.Select(sr => sr.Services.Application).Distinct().Count(),
                    InstanceCount = p.FirstOrDefault(sr => !sr.Services.IsSilkyService())?.GetInstanceCount() ?? 0,
                    LocalServiceEntriesCount = p.SelectMany(sr => sr.Services.ServiceEntries).Distinct().Count(),
                    AppServiceCount = p.Select(sr => sr.Services.ServiceName).Distinct().Count(),
                    HasWsService = p.Any(sr => sr.Services.ServiceProtocol == ServiceProtocol.Ws)
                });
            return hosts.ToArray();
        }

        public GetDetailHostOutput GetHostDetail(string hostName)
        {
            var detailHostOutput = new GetDetailHostOutput()
            {
                HostName = hostName
            };
            var appServices = _serverRouteCache.ServiceRoutes.Where(p => p.Services.GetHostName() == hostName)
                .OrderBy(p => p.Services.ServiceName);

            detailHostOutput.AppServiceEntries = appServices
                .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Tcp).SelectMany(
                    sr =>
                    {
                        var serviceEntryOutputs = new List<ServiceEntryOutput>();
                        foreach (var sed in sr.Services.ServiceEntries)
                        {
                            var se = _serviceEntryManager.GetServiceEntry(sed.Id);
                            if (se != null)
                            {
                                var seOutput = new ServiceEntryOutput()
                                {
                                    ServiceProtocol = se.ServiceEntryDescriptor.ServiceProtocol,
                                    HostName = sr.Services.GetHostName(),
                                    Application = se.ServiceEntryDescriptor.Application,
                                    ServiceName = se.ServiceEntryDescriptor.ServiceName,
                                    ServiceId = se.ServiceEntryDescriptor.ServiceId,
                                    ServiceEntryId = se.ServiceEntryDescriptor.Id,
                                    MultipleServiceKey = sr.MultiServiceKeys(),
                                    Author = se.ServiceEntryDescriptor.GetAuthor(),
                                    WebApi = se.GovernanceOptions.ProhibitExtranet ? null : se.Router.RoutePath,
                                    HttpMethod = se.GovernanceOptions.ProhibitExtranet ? null : se.Router.HttpMethod,
                                    ProhibitExtranet = se.GovernanceOptions.ProhibitExtranet,
                                    Method = se.MethodInfo.Name
                                };
                                serviceEntryOutputs.Add(seOutput);
                            }
                        }

                        return serviceEntryOutputs;
                    }).ToArray();

            detailHostOutput.WsServices = appServices
                .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Ws).Select(
                    p => new WsAppServiceOutput()
                    {
                        AppService = p.Services.ServiceName,
                        WsPath = p.Services.GetWsPath(),
                        ServiceProtocol = p.Services.ServiceProtocol
                    }).ToArray();
            return detailHostOutput;
        }

        public IReadOnlyCollection<GetServiceOutput> GetServices(string hostName)
        {
            var appServiceGroups = _serverRouteCache.ServiceRoutes
                .Where(p => !p.Services.GetHostName().IsNullOrEmpty())
                .WhereIf(!hostName.IsNullOrEmpty(), p => p.Services.GetHostName() == hostName)
                .OrderBy(p => p.Services.ServiceName).GroupBy(p =>
                    p.Services);
            var services = new List<GetServiceOutput>();
            foreach (var appServiceGroup in appServiceGroups)
            {
                services.Add(new GetServiceOutput()
                {
                    HostName = appServiceGroup.Key.GetHostName(),
                    ServiceId = appServiceGroup.Key.Id,
                    ServiceName = appServiceGroup.Key.ServiceName,
                    Application = appServiceGroup.Key.Application,
                    InstanceCount = _serverRouteCache.GetServiceRoute(appServiceGroup.Key.Id).GetInstanceCount()
                });
            }

            return services.ToArray();
        }

        public PagedList<GetHostInstanceOutput> GetHostInstances(string hostName,
            GetHostInstanceInput input)
        {
            var hostAddresses = _serverRouteCache.ServiceRoutes
                    .Where(p => p.Services.GetHostName() == hostName)
                    .WhereIf(input.ServiceProtocol.HasValue, p => p.Services.ServiceProtocol == input.ServiceProtocol)
                    .SelectMany(p => p.Endpoints)
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
            var gateway =
                _serverRouteCache.ServiceRoutes.First(p => p.Services.GetHostName() == EngineContext.Current.HostName);
            var gatewayOutput = new GetGatewayOutput()
            {
                HostName = gateway.Services.GetHostName(),
                InstanceCount = gateway.Endpoints.Select(p => new { Address = p.Host, p.Port }).Distinct().Count(),
                SupportServiceCount = _serverRouteCache.ServiceRoutes.Select(p => p.Services).Count(),
                SupportServiceEntryCount = _serverRouteCache.ServiceRoutes
                    .SelectMany(p => p.Services.ServiceEntries).Count(),
                ExistWebSocketService =
                    _serverRouteCache.ServiceRoutes.Any(p =>
                        p.Services.ServiceProtocol == ServiceProtocol.Ws),
                SupportServices = _serverRouteCache.ServiceRoutes.Select(p => p.Services)
                    .Select(p => p.ServiceName)
            };
            return gatewayOutput;
        }

        public PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input)
        {
            var gateway =
                _serverRouteCache.ServiceRoutes.First(p => p.Services.GetHostName() == EngineContext.Current.HostName);

            var gatewayInstances = new List<GetGatewayInstanceOutput>();
            foreach (var addressDescriptor in gateway.Endpoints)
            {
                var gatewayInstance = new GetGatewayInstanceOutput()
                {
                    HostName = gateway.Services.GetHostName(),
                    Address = addressDescriptor.Host,
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
                    var serviceRoute = p.GetServiceRoute();
                    var serviceEntryOutput = new GetServiceEntryOutput()
                    {
                        ServiceId = p.ServiceId,
                        ServiceName = p.ServiceEntryDescriptor.ServiceName,
                        ServiceEntryId = p.Id,
                        Author = p.ServiceEntryDescriptor.GetAuthor(),
                        HostName = serviceRoute?.Services.GetHostName(),
                        Application = p.ServiceEntryDescriptor.Application,
                        WebApi = p.GovernanceOptions.ProhibitExtranet ? "" : p.Router.RoutePath,
                        HttpMethod = p.GovernanceOptions.ProhibitExtranet ? null : p.Router.HttpMethod,
                        ProhibitExtranet = p.GovernanceOptions.ProhibitExtranet,
                        Method = p.MethodInfo.Name,
                        MultipleServiceKey = serviceRoute?.MultiServiceKeys() == true,
                        IsEnable = serviceRoute != null &&
                                   serviceRoute.Endpoints.Any(am => SocketCheck.TestConnection(am.Host, am.Port)),
                        ServiceRouteCount = serviceRoute?.Endpoints.Length ?? 0,
                        IsDistributeTransaction = p.IsTransactionServiceEntry()
                    };
                    return serviceEntryOutput;
                }).Where(p => !p.Application.IsNullOrEmpty())
                .WhereIf(!input.Application.IsNullOrEmpty(), p => input.Application.Equals(p.Application))
                .WhereIf(!input.ServiceId.IsNullOrEmpty(), p => input.ServiceId.Equals(p.ServiceId))
                .WhereIf(!input.ServiceEntryId.IsNullOrEmpty(), p => input.ServiceEntryId.Equals(p.ServiceEntryId))
                .WhereIf(!input.Name.IsNullOrEmpty(), p => p.ServiceEntryId.Contains(input.Name))
                .WhereIf(input.IsEnable.HasValue, p => p.IsEnable == input.IsEnable)
                .OrderBy(p => p.Application).ThenBy(p => p.ServiceId);

            return serviceEntryOutputs.ToPagedList(input.PageIndex, input.PageSize);
        }

        public GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceEntryId)
        {
            var serviceEntry = _serviceEntryManager.GetAllEntries().FirstOrDefault(p => p.Id == serviceEntryId);
            if (serviceEntry == null)
            {
                throw new BusinessException($"No service entry for {serviceEntryId} exists");
            }

            var serviceRoute = serviceEntry.GetServiceRoute();

            var serviceEntryOutput = new GetServiceEntryDetailOutput()
            {
                HostName = serviceRoute?.Services.GetHostName(),
                ServiceEntryId = serviceEntry.Id,
                ServiceId = serviceEntry.ServiceEntryDescriptor.ServiceId,
                ServiceName = serviceEntry.ServiceEntryDescriptor.ServiceName,
                Author = serviceEntry.ServiceEntryDescriptor.GetAuthor(),
                Application = serviceRoute?.Services.Application,
                WebApi = serviceEntry.GovernanceOptions.ProhibitExtranet ? "" : serviceEntry.Router.RoutePath,
                HttpMethod = serviceEntry.GovernanceOptions.ProhibitExtranet ? null : serviceEntry.Router.HttpMethod,
                ProhibitExtranet = serviceEntry.GovernanceOptions.ProhibitExtranet,
                Method = serviceEntry.MethodInfo.Name,
                MultipleServiceKey = serviceRoute?.MultiServiceKeys() == true,
                IsEnable = serviceRoute != null &&
                           serviceRoute.Endpoints.Any(p => SocketCheck.TestConnection(p.Host, p.Port)),
                ServiceRouteCount = serviceRoute?.Endpoints.Length ?? 0,
                GovernanceOptions = serviceEntry.GovernanceOptions,
                CacheTemplates = serviceEntry.CustomAttributes.OfType<ICachingInterceptProvider>().Select(p =>
                    new ServiceEntryCacheTemplateOutput()
                    {
                        KeyTemplete = p.KeyTemplete,
                        OnlyCurrentUserData = p.OnlyCurrentUserData,
                        CachingMethod = p.CachingMethod
                    }).ToArray(),
                ServiceKeys = serviceRoute?.Services.GetServiceKeys()?.Select(p => new ServiceKeyOutput()
                {
                    Name = p.Key,
                    Weight = p.Value
                }).ToArray(),
                IsDistributeTransaction = serviceEntry.IsTransactionServiceEntry()
            };

            return serviceEntryOutput;
        }

        public PagedList<GetServiceEntryRouteOutput> GetServiceEntryRoutes(string serviceEntryId, int pageIndex = 1,
            int pageSize = 10)
        {
            var serviceEntry = _serviceEntryManager.GetAllEntries().FirstOrDefault(p => p.Id == serviceEntryId);
            if (serviceEntry == null)
            {
                throw new BusinessException($"No service entry for {serviceEntryId} exists");
            }

            var serviceEntryInstances = new List<GetServiceEntryRouteOutput>();

            var serviceRoute = serviceEntry.GetServiceRoute();
            if (serviceRoute != null)
            {
                foreach (var address in serviceRoute.Endpoints)
                {
                    var serviceEntryInstance = new GetServiceEntryRouteOutput()
                    {
                        ServiceName = serviceEntry.ServiceEntryDescriptor.ServiceName,
                        ServiceId = serviceEntry.ServiceId,
                        ServiceEntryId = serviceEntry.Id,
                        Address = address.IPEndPoint.ToString(),
                        IsEnable = address.Enabled,
                        IsHealth = SocketCheck.TestConnection(address.IPEndPoint),
                        ServiceProtocol = address.ServiceProtocol
                    };
                    serviceEntryInstances.Add(serviceEntryInstance);
                }
            }


            return serviceEntryInstances.ToPagedList(pageIndex, pageSize);
        }

        public async Task<GetInstanceDetailOutput> GetInstanceDetail(string address)
        {
            if (!Regex.IsMatch(address, ipEndpointRegex))
            {
                throw new BusinessException($"{address} incorrect rpc address format");
            }

            var addressInfo = address.Split(":");
            if (!SocketCheck.TestConnection(addressInfo[0], int.Parse(addressInfo[1])))
            {
                throw new BusinessException($"{address} is unHealth");
            }

            RpcContext.Context.SetAttachment(AttachmentKeys.SelectedServerEndpoint, address);

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(getInstanceSupervisorServiceEntryId);
            if (serviceEntry == null)
            {
                throw new BusinessException($"Not find serviceEntry by {getInstanceSupervisorServiceEntryId}");
            }

            var result =
                (await _remoteExecutor.Execute(serviceEntry, Array.Empty<object>(), null)) as GetInstanceDetailOutput;
            if (result?.Address != address)
            {
                throw new SilkyException("The rpc address of the routing instance is wrong");
            }

            return result;
        }

        public async Task<PagedList<ServiceEntryHandleInfo>> GetServiceEntryHandleInfos(string address,
            PagedRequestDto input)
        {
            if (!Regex.IsMatch(address, ipEndpointRegex))
            {
                throw new BusinessException($"{address} incorrect rpcAddress format");
            }

            var addressInfo = address.Split(":");
            if (!SocketCheck.TestConnection(addressInfo[0], int.Parse(addressInfo[1])))
            {
                throw new BusinessException($"{address} is unHealth");
            }

            RpcContext.Context.SetAttachment(AttachmentKeys.SelectedServerEndpoint, address);
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(getGetServiceEntrySupervisorServiceHandle);
            if (serviceEntry == null)
            {
                throw new BusinessException($"Not find serviceEntry by {getGetServiceEntrySupervisorServiceHandle}");
            }

            IReadOnlyCollection<ServiceEntryHandleInfo> result;
            if (serviceEntry.IsLocal)
            {
                result =
                    await _localExecutor.Execute(serviceEntry, Array.Empty<object>()) as
                        IReadOnlyCollection<ServiceEntryHandleInfo>;
            }
            else
            {
                result =
                    await _remoteExecutor.Execute(serviceEntry, Array.Empty<object>(), null) as
                        IReadOnlyCollection<ServiceEntryHandleInfo>;
            }

            return result.ToPagedList(input.PageIndex, input.PageSize);
        }


        public async Task<PagedList<ServiceEntryInvokeInfo>> GetServiceEntryInvokeInfos(string address,
            PagedRequestDto input)
        {
            if (!Regex.IsMatch(address, ipEndpointRegex))
            {
                throw new BusinessException($"{address} incorrect rpc address format");
            }

            var addressInfo = address.Split(":");
            if (!SocketCheck.TestConnection(addressInfo[0], int.Parse(addressInfo[1])))
            {
                throw new BusinessException($"{address} is unHealth");
            }

            RpcContext.Context.SetAttachment(AttachmentKeys.SelectedServerEndpoint, address);

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(getGetServiceEntrySupervisorServiceInvoke);
            if (serviceEntry == null)
            {
                throw new BusinessException($"Not find serviceEntry by {getGetServiceEntrySupervisorServiceInvoke}");
            }

            IReadOnlyCollection<ServiceEntryInvokeInfo> result;
            if (serviceEntry.IsLocal)
            {
                result =
                    await _localExecutor.Execute(serviceEntry, Array.Empty<object>()) as
                        IReadOnlyCollection<ServiceEntryInvokeInfo>;
            }
            else
            {
                result =
                    await _remoteExecutor.Execute(serviceEntry, Array.Empty<object>(), null) as
                        IReadOnlyCollection<ServiceEntryInvokeInfo>;
            }

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
                Count = _serverRouteCache.ServiceRoutes
                    .Where(p => !p.Services.Application.IsNullOrEmpty() &&
                                p.Services.Application != typeof(IRpcAppService).Name)
                    .GroupBy(p => p.Services.Application).Select(p => p.Key)
                    .Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceInstance",
                Title = "微服务应用实例",
                Count = _serverRouteCache.ServiceRoutes
                    .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Tcp)
                    .SelectMany(p => p.Endpoints)
                    .Select(p => new { Address = p.Host, p.Port }).Distinct().Count()
            });

            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketServiceInstance",
                Title = "支持WebSocket的应用实例",
                Count = _serverRouteCache.ServiceRoutes
                    .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Ws)
                    .SelectMany(p => p.Endpoints)
                    .Select(p => new { Address = p.Host, p.Port }).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "Services",
                Title = "应用服务",
                Count = _serverRouteCache.ServiceRoutes
                    .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Tcp)
                    .Select(p => p.Services.ServiceName).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketService",
                Title = "WebSocket服务",
                Count = _serverRouteCache.ServiceRoutes
                    .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Ws)
                    .Select(p => p.Services.Id).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceEntry",
                Title = "服务条目",
                Count = _serviceEntryManager.GetAllEntries()
                    .Select(p => p.ServiceEntryDescriptor.Id).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "RegistryCenter",
                Title = "服务注册中心",
                Count = _registerCenterHealthProvider.GetRegistryCenterHealthInfo().Count
            });

            return getProfileOutputs.Where(p => p.Count > 0).ToArray();
        }

        public IReadOnlyCollection<GetExternalRouteOutput> GetExternalRoutes()
        {
            var externalRoutes = new List<GetExternalRouteOutput>();
            var dashboardOptions = EngineContext.Current.GetOptionsSnapshot<DashboardOptions>();
            if (dashboardOptions.ExternalLinks != null && dashboardOptions.ExternalLinks.Any())
            {
                var externalRoute = CreateExternalRoute("/external");
                externalRoute.Meta["Icon"] = "el-icon-link";
                externalRoute.Meta["Title"] = "外部链接";
                externalRoute.Meta["IsLayout"] = true;
                externalRoute.Meta["ShowLink"] = true;
                externalRoute.Meta["SavedPosition"] = false;
                externalRoute.Name = "external";
                foreach (var externalLink in dashboardOptions.ExternalLinks)
                {
                    var externalRouteChild = CreateExternalRoute(externalLink.Path);
                    externalRouteChild.Meta["Icon"] = externalLink.Icon ?? "el-icon-link";
                    externalRouteChild.Meta["Title"] = externalLink.Title;
                    externalRouteChild.Meta["ShowLink"] = true;
                    externalRouteChild.Meta["ExternalLink"] = true;
                    externalRoute.Meta["SavedPosition"] = false;
                    externalRoute.Children.Add(externalRouteChild);
                }

                externalRoutes.Add(externalRoute);
            }

            return externalRoutes.ToArray();
        }


        private GetExternalRouteOutput CreateExternalRoute(string path)
        {
            var externalRoute = new GetExternalRouteOutput()
            {
                Path = path,
                Meta = new Dictionary<string, object>()
            };
            return externalRoute;
        }
    }
}