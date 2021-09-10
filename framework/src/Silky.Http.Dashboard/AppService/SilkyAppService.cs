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
using Silky.Rpc.Configuration;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.CachingIntercept;
using Silky.Rpc.Utils;

namespace Silky.Http.Dashboard.AppService
{
    public class SilkyAppService : ISilkyAppService
    {
        private readonly ServiceRouteCache _serviceRouteCache;
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IRemoteServiceExecutor _serviceExecutor;
        private readonly IRegisterCenterHealthProvider _registerCenterHealthProvider;
        private readonly RegistryCenterOptions _registryCenterOptions;


        private const string ipEndpointRegex =
            @"([0-9]|[1-9]\d{1,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])";

        private const string getInstanceSupervisorServiceEntryId =
            "Silky.Rpc.AppServices.IRpcAppService.GetInstanceDetail";

        private const string getGetServiceEntrySupervisorServiceHandle =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntryHandleInfos";

        private const string getGetServiceEntrySupervisorServiceInvoke =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntryInvokeInfos";

        public SilkyAppService(
            ServiceRouteCache serviceRouteCache,
            IServiceEntryManager serviceEntryManager,
            IServiceEntryLocator serviceEntryLocator,
            IRemoteServiceExecutor serviceExecutor,
            IRegisterCenterHealthProvider registerCenterHealthProvider,
            IOptions<RegistryCenterOptions> registryCenterOptions)
        {
            _serviceRouteCache = serviceRouteCache;
            _serviceEntryManager = serviceEntryManager;

            _serviceExecutor = serviceExecutor;
            _registerCenterHealthProvider = registerCenterHealthProvider;
            _serviceEntryLocator = serviceEntryLocator;
            _registryCenterOptions = registryCenterOptions.Value;
        }

        public PagedList<GetApplicationOutput> GetApplications(PagedRequestDto input)
        {
            return GetAllApplications().ToPagedList(input.PageIndex, input.PageSize);
        }

        public IReadOnlyCollection<GetApplicationOutput> GetAllApplications()
        {
            var serviceRoutes = _serviceRouteCache.ServiceRoutes;
            var hosts = serviceRoutes.GroupBy(p => p.Services.Application)
                .Select(p => new GetApplicationOutput()
                {
                    HostName = p.Key,
                    InstanceCount = p.Max(sr => sr.Addresses.Length),
                    ServiceEntriesCount =
                        p.Select(sr => _serviceEntryManager.GetServiceEntries(sr.Services.Id)?.Count ?? 0)
                            .Sum(),
                    AppServiceCount = p.GroupBy(sr => sr.Services.ServiceName).Count(),
                    HasWsService = p.Any(sr => sr.Services.ServiceProtocol == ServiceProtocol.Ws)
                });
            return hosts.ToArray();
        }

        public GetDetailApplicationOutput GetApplicationDetail(string appName)
        {
            var detailHostOutput = new GetDetailApplicationOutput()
            {
                HostName = appName
            };
            var allServiceEntries = _serviceEntryManager.GetAllEntries()
                .Where(p => p.ServiceEntryDescriptor.Application == appName);
            var appServices = _serviceRouteCache.ServiceRoutes.Where(p => p.Services.Application == appName)
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
                                    ServiceName = se.ServiceEntryDescriptor.ServiceName,
                                    ServiceId = se.ServiceEntryDescriptor.ServiceId,
                                    ServiceEntryId = se.ServiceEntryDescriptor.Id,
                                    MultipleServiceKey = se.MultipleServiceKey,
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

        public IReadOnlyCollection<GetServiceOutput> GetServices(string appName)
        {
            var appServiceGroups = _serviceRouteCache.ServiceRoutes
                .WhereIf(!appName.IsNullOrEmpty(), p => p.Services.Application == appName)
                .OrderBy(p => p.Services.ServiceName).GroupBy(p =>
                    new
                    {
                        HostName = p.Services.Application, ServiceName = p.Services.ServiceName,
                        ServiceId = p.Services.Id,
                    });
            var services = new List<GetServiceOutput>();
            foreach (var appServiceGroup in appServiceGroups)
            {
                services.Add(new GetServiceOutput()
                {
                    ServiceId = appServiceGroup.Key.ServiceId,
                    ServiceName = appServiceGroup.Key.ServiceName,
                    Application = appServiceGroup.Key.HostName,
                    InstanceCount = _serviceRouteCache.ServiceRoutes.Where(p =>
                        p.Services.ServiceName == appServiceGroup.Key.ServiceName
                        && p.Services.Application == appServiceGroup.Key.HostName).Max(p => p.Addresses.Length)
                });
            }

            return services.ToArray();
        }

        public PagedList<GetApplicationInstanceOutput> GetApplicationInstances(string appName,
            GetApplicationInstanceInput input)
        {
            var hostAddresses = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.Services.Application == appName &&
                                p.Services.ServiceProtocol == input.ServiceProtocol)
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
            var gateway = _serviceRouteCache.ServiceRoutes.First(p => p.Services.Application == EngineContext.Current.HostName);
            var gatewayOutput = new GetGatewayOutput()
            {
                HostName = gateway.Services.Application,
                InstanceCount = gateway.Addresses.Select(p => new { p.Address, p.Port }).Distinct().Count(),
                SupportServiceCount = _serviceRouteCache.ServiceRoutes.Select(p=> p.Services).Count(),
                SupportServiceEntryCount = _serviceRouteCache.ServiceRoutes
                    .SelectMany(p => p.Services.ServiceEntries).Count(),
                ExistWebSocketService =
                    _serviceRouteCache.ServiceRoutes.Any(p =>
                        p.Services.ServiceProtocol == ServiceProtocol.Ws),
                SupportServices = _serviceRouteCache.ServiceRoutes.SelectMany(p=> p.Services.ServiceEntries).Select(p=>p.ServiceName)
            };
            return gatewayOutput;
        }

        public PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input)
        {
            var gateway = _serviceRouteCache.ServiceRoutes.First(p => p.Services.Application == EngineContext.Current.HostName);

            var gatewayInstances = new List<GetGatewayInstanceOutput>();
            foreach (var addressDescriptor in gateway.Addresses)
            {
                var gatewayInstance = new GetGatewayInstanceOutput()
                {
                    HostName = gateway.Services.Application,
                    Address = addressDescriptor.Address,
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
                        _serviceRouteCache.GetServiceRoute(p.ServiceId);
                    var serviceEntryOutput = new GetServiceEntryOutput()
                    {
                        ServiceId = p.ServiceId,
                        ServiceName = p.ServiceEntryDescriptor.ServiceName,
                        ServiceEntryId = p.Id,
                        Author = p.ServiceEntryDescriptor.GetAuthor(),
                        Application = serviceRoute?.Services.Application,
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

            var serviceRoute =
                _serviceRouteCache.ServiceRoutes.FirstOrDefault(sr => sr.Services.Id == serviceEntryId);
            var serviceEntryOutput = new GetServiceEntryDetailOutput()
            {
                ServiceEntryId = serviceEntry.Id,
                ServiceId = serviceEntry.ServiceEntryDescriptor.ServiceId,
                ServiceName = serviceEntry.ServiceEntryDescriptor.ServiceName,
                Author = serviceEntry.ServiceEntryDescriptor.GetAuthor(),
                Application = serviceRoute?.Services.Application,
                WebApi = serviceEntry.GovernanceOptions.ProhibitExtranet ? "" : serviceEntry.Router.RoutePath,
                HttpMethod = serviceEntry.GovernanceOptions.ProhibitExtranet ? null : serviceEntry.Router.HttpMethod,
                ProhibitExtranet = serviceEntry.GovernanceOptions.ProhibitExtranet,
                Method = serviceEntry.MethodInfo.Name,
                MultipleServiceKey = serviceEntry.MultipleServiceKey,
                IsEnable = serviceRoute != null &&
                           serviceRoute.Addresses.Any(p => SocketCheck.TestConnection(p.Address, p.Port)),
                ServiceRouteCount = serviceRoute?.Addresses.Length ?? 0,
                GovernanceOptions = serviceEntry.GovernanceOptions,
                CacheTemplates = serviceEntry.CustomAttributes.OfType<ICachingInterceptProvider>().Select(p =>
                    new ServiceEntryCacheTemplateOutput()
                    {
                        KeyTemplete = p.KeyTemplete,
                        OnlyCurrentUserData = p.OnlyCurrentUserData,
                        CachingMethod = p.CachingMethod
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

            var serviceRoute =
                _serviceRouteCache.GetServiceRoute(serviceEntry.ServiceId);

            if (serviceRoute != null)
            {
                foreach (var address in serviceRoute.Addresses)
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
                throw new BusinessException($"{address} incorrect address format");
            }

            var addressInfo = address.Split(":");
            if (!SocketCheck.TestConnection(addressInfo[0], int.Parse(addressInfo[1])))
            {
                throw new BusinessException($"{address} is unHealth");
            }

            RpcContext.Context.SetAttachment(AttachmentKeys.SelectedAddress, address);

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(getInstanceSupervisorServiceEntryId);
            if (serviceEntry == null)
            {
                throw new BusinessException($"Not find serviceEntry by {getInstanceSupervisorServiceEntryId}");
            }

            var result =
                (await _serviceExecutor.Execute(serviceEntry, Array.Empty<object>(), null)) as GetInstanceDetailOutput;
            if (result?.Address != address)
            {
                throw new SilkyException("The address of the routing instance is wrong");
            }

            return result;
        }

        public async Task<PagedList<ServiceEntryHandleInfo>> GetServiceEntryHandleInfos(string address,
            PagedRequestDto input)
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
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(getGetServiceEntrySupervisorServiceHandle);
            if (serviceEntry == null)
            {
                throw new BusinessException($"Not find serviceEntry by {getGetServiceEntrySupervisorServiceHandle}");
            }

            var result =
                await _serviceExecutor.Execute(serviceEntry, Array.Empty<object>(), null) as
                    IReadOnlyCollection<ServiceEntryHandleInfo>;
            return result.ToPagedList(input.PageIndex, input.PageSize);
        }


        public async Task<PagedList<ServiceEntryInvokeInfo>> GetServiceEntryInvokeInfos(string address,
            PagedRequestDto input)
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

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(getGetServiceEntrySupervisorServiceInvoke);
            if (serviceEntry == null)
            {
                throw new BusinessException($"Not find serviceEntry by {getGetServiceEntrySupervisorServiceInvoke}");
            }

            var result =
                await _serviceExecutor.Execute(serviceEntry, Array.Empty<object>(), null) as
                    IReadOnlyCollection<ServiceEntryInvokeInfo>;
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
                    .Where(p => !p.Services.Application.IsNullOrEmpty() &&
                                p.Services.Application != typeof(IRpcAppService).Name)
                    .GroupBy(p => p.Services.Application).Select(p => p.Key)
                    .Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceInstance",
                Title = "微服务应用实例",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Tcp)
                    .SelectMany(p => p.Addresses)
                    .Select(p => new { p.Address, p.Port }).Distinct().Count()
            });

            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketServiceInstance",
                Title = "支持WebSocket的应用实例",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Ws)
                    .SelectMany(p => p.Addresses)
                    .Select(p => new { p.Address, p.Port }).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "Service",
                Title = "应用服务",
                Count = _serviceRouteCache.ServiceRoutes
                    .Where(p => p.Services.ServiceProtocol == ServiceProtocol.Tcp)
                    .Select(p => p.Services.ServiceName).Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketService",
                Title = "WebSocket服务",
                Count = _serviceRouteCache.ServiceRoutes
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