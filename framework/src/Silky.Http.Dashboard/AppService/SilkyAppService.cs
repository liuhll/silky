using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Runtime.Rpc;
using Silky.HealthChecks.Rpc.ServerCheck;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Http.Dashboard.Configuration;
using Silky.Rpc.CachingInterceptor.Providers;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService
{
    public class SilkyAppService : ISilkyAppService
    {
        private readonly IServerManager _serverManager;
        private readonly IServiceManager _serviceManager;
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IRegisterCenterHealthProvider _registerCenterHealthProvider;
        private readonly IServerHealthCheck _serverHealthCheck;
        private readonly IAppointAddressInvoker _appointAddressInvoker;


        private const string ipEndpointRegex =
            @"([0-9]|[1-9]\d{1,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])";

        private const string getInstanceSupervisorServiceEntryId =
            "Silky.Rpc.AppService.IRpcAppService.GetInstanceDetail_Get";

        private const string getGetServiceEntrySupervisorServiceHandleServiceEntryId =
            "Silky.Rpc.AppService.IRpcAppService.GetServiceEntryHandleInfos_Get";

        private const string getGetServiceEntrySupervisorServiceInvokeServiceEntryId =
            "Silky.Rpc.AppService.IRpcAppService.GetServiceEntryInvokeInfos_Get";

        public SilkyAppService(
            IServerManager serverManager,
            IServiceManager serviceManager,
            IServiceEntryManager serviceEntryManager,
            IServiceEntryLocator serviceEntryLocator,
            IRegisterCenterHealthProvider registerCenterHealthProvider,
            IServerHealthCheck serverHealthCheck,
            IAppointAddressInvoker appointAddressInvoker)
        {
            _serverManager = serverManager;
            _serviceEntryManager = serviceEntryManager;

            _registerCenterHealthProvider = registerCenterHealthProvider;
            _serverHealthCheck = serverHealthCheck;
            _appointAddressInvoker = appointAddressInvoker;
            _serviceManager = serviceManager;
            _serviceEntryLocator = serviceEntryLocator;
        }

        public PagedList<GetHostOutput> GetHosts(PagedRequestDto input)
        {
            return GetAllHosts().ToPagedList(input.PageIndex, input.PageSize);
        }

        public IReadOnlyCollection<GetHostOutput> GetAllHosts()
        {
            var serverDescriptors = _serverManager
                .ServerDescriptors
                .Where(p => p.Endpoints.Any(e => e.IsInstanceEndpoint()))
                .Select(p => new GetHostOutput()
                {
                    AppServiceCount = p.Services.Length,
                    InstanceCount = p.Endpoints.Count(e => e.IsInstanceEndpoint()),
                    HostName = p.HostName,
                    ServiceProtocols = p.Endpoints.Select(e => e.ServiceProtocol).Distinct().ToArray(),
                    ServiceEntriesCount = p.Services.SelectMany(s => s.ServiceEntries).Count()
                });
            return serverDescriptors.ToArray();
        }

        public ServerDescriptor GetHostDetail(string hostName)
        {
            var serverDescriptor = _serverManager.GetServerDescriptor(hostName);
            if (serverDescriptor == null)
            {
                throw new BusinessException($"There is no server for {hostName}");
            }

            return serverDescriptor;
        }

        public async Task<PagedList<GetHostInstanceOutput>> GetHostInstances(string hostName,
            GetHostInstanceInput input)
        {
            var server = _serverManager.GetServer(hostName);
            if (server == null)
            {
                throw new BusinessException($"There is no server for {hostName}");
            }

            var hostInstances = new List<GetHostInstanceOutput>();

            foreach (var endpoint in server.Endpoints)
            {
                if (input.ServiceProtocol.HasValue && input.ServiceProtocol != endpoint.ServiceProtocol)
                {
                    continue;
                }

                var isHealth = await _serverHealthCheck.IsHealth(endpoint);
                hostInstances.Add(new GetHostInstanceOutput()
                {
                    HostName = server.HostName,
                    IsHealth = isHealth,
                    Address = endpoint.GetAddress(),
                    Host = endpoint.Host,
                    Port = endpoint.Port,
                    IsEnable = endpoint.Enabled,
                    LastDisableTime = endpoint.LastDisableTime,
                    ServiceProtocol = endpoint.ServiceProtocol,
                });
            }

            return hostInstances.ToPagedList(input.PageIndex, input.PageSize);
        }

        public PagedList<GetWebSocketServiceOutput> GetWebSocketServices(string hostName, PagedRequestDto input)
        {
            var webSocketServiceOutputs = new List<GetWebSocketServiceOutput>();

            var server = _serverManager.GetServer(hostName);
            var wsServices = server.Services.Where(p => p.ServiceProtocol == ServiceProtocol.Ws);
            var endpoints = server.Endpoints.Where(p => p.ServiceProtocol == ServiceProtocol.Ws);
            foreach (var wsService in wsServices)
            {
                foreach (var endpoint in endpoints)
                {
                    var webSocketServiceOutput = new GetWebSocketServiceOutput()
                    {
                        HostName = server.HostName,
                        ServiceId = wsService.Id,
                        ServiceName = wsService.ServiceName,
                        Address = endpoint.GetAddress(),
                        ProxyAddress = $"{GetWebSocketProxyAddress()}{wsService.GetWsPath()}",
                        Path = wsService.GetWsPath()
                    };
                    webSocketServiceOutputs.Add(webSocketServiceOutput);
                }
            }

            return webSocketServiceOutputs.ToPagedList(input.PageIndex, input.PageSize);
        }

        private string GetWebSocketProxyAddress()
        {
            var webEndpoint = RpcEndpointHelper.GetLocalWebEndpoint();
            if (webEndpoint == null)
            {
                return string.Empty;
            }

            var wsServiceProtocol = webEndpoint.ServiceProtocol == ServiceProtocol.Https
                ? ServiceProtocol.Wss
                : ServiceProtocol.Ws;
            return $"{wsServiceProtocol.ToString().ToLower()}://{webEndpoint.GetAddress()}";
        }

        public IReadOnlyCollection<GetServiceOutput> GetServices(string hostName)
        {
            return _serverManager.ServerDescriptors
                .WhereIf(!hostName.IsNullOrEmpty(), p => p.HostName.Equals(hostName))
                .SelectMany(p => p.Services.Select(s => new GetServiceOutput()
                {
                    ServiceName = s.ServiceName,
                    ServiceId = s.Id,
                    ServiceProtocol = s.ServiceProtocol
                }))
                .Distinct()
                .ToArray();
        }

        public GetGatewayOutput GetGateway()
        {
            var gateway = _serverManager.GetServer(EngineContext.Current.HostName);
            return new GetGatewayOutput()
            {
                HostName = gateway.HostName,
                InstanceCount = gateway.Endpoints.Count(p => p.ServiceProtocol.IsHttp()),
                SupportServiceCount = _serviceManager.GetAllService().Count,
                SupportServiceEntryCount = _serviceEntryManager.GetAllEntries().Count
            };
        }

        public PagedList<GetGatewayInstanceOutput> GetGatewayInstances(PagedRequestDto input)
        {
            var gateway = _serverManager.GetServer(EngineContext.Current.HostName);

            var gatewayInstances = new List<GetGatewayInstanceOutput>();
            var gatewayEndpoints = gateway.Endpoints.Where(p => p.ServiceProtocol.IsHttp());
            foreach (var addressDescriptor in gatewayEndpoints)
            {
                var gatewayInstance = new GetGatewayInstanceOutput()
                {
                    HostName = gateway.HostName,
                    Address =
                        $"{addressDescriptor.ServiceProtocol}://{addressDescriptor.Host}:{addressDescriptor.Port}"
                            .ToLower(),
                    Port = addressDescriptor.Port,
                    ServiceProtocol = addressDescriptor.ServiceProtocol
                };
                gatewayInstances.Add(gatewayInstance);
            }

            return gatewayInstances.ToPagedList(input.PageIndex, input.PageSize);
        }


        public PagedList<ServiceEntryDescriptor> GetGatewayServiceEntries(GetGatewayServiceEntryInput input)
        {
            var serviceEntries = _serviceEntryManager
                    .GetAllEntries()
                    .Select(p => p.ServiceEntryDescriptor)
                    .Where(p => !p.ProhibitExtranet)
                    .WhereIf(!input.ServiceId.IsNullOrEmpty(), p => p.ServiceId.Equals(input.ServiceId))
                    .WhereIf(!input.SearchKey.IsNullOrEmpty(), p =>
                        p.ServiceId.Contains(input.SearchKey) ||
                        p.ServiceName.Contains(input.SearchKey) ||
                        p.Id.Contains(input.SearchKey) ||
                        p.WebApi.Contains(input.SearchKey)
                    )
                ;
            return serviceEntries.ToPagedList(input.PageIndex, input.PageSize);
        }

        public ICollection<GetServiceOutput> GetGatewayServices()
        {
            var services = _serviceManager.GetAllService()
                .Where(p => !p.ServiceDescriptor.IsSilkyService())
                .Select(p => new GetServiceOutput()
                {
                    ServiceId = p.Id,
                    ServiceName = p.ServiceDescriptor.ServiceName,
                    ServiceProtocol = p.ServiceProtocol
                }).Distinct().ToList();
            return services;
        }

        public PagedList<GetServiceEntryOutput> GetServiceEntries(GetServiceEntryInput input)
        {
            var serviceEntryOutputs = GetAllServiceEntryFromCache();

            serviceEntryOutputs = serviceEntryOutputs
                .WhereIf(!input.HostName.IsNullOrEmpty(),
                    p => input.HostName.Equals(p.HostName))
                .WhereIf(!input.ServiceId.IsNullOrEmpty(),
                    p => input.ServiceId.Equals(p.ServiceId))
                .WhereIf(!input.ServiceEntryId.IsNullOrEmpty(),
                    p => input.ServiceEntryId.Equals(p.ServiceEntryId))
                .WhereIf(!input.SearchKey.IsNullOrEmpty(),
                    p =>
                        p.ServiceEntryId.Contains(input.SearchKey) ||
                        p.Author?.Contains(input.SearchKey) == true ||
                        p.WebApi?.Contains(input.SearchKey) == true ||
                        p.Method.Contains(input.SearchKey)
                )
                .WhereIf(input.IsEnable.HasValue, p => p.IsEnable == input.IsEnable)
                .WhereIf(input.IsAllowAnonymous.HasValue, p => p.IsAllowAnonymous == input.IsAllowAnonymous)
                .WhereIf(input.ProhibitExtranet.HasValue, p => p.ProhibitExtranet == input.ProhibitExtranet)
                .WhereIf(input.IsDistributeTransaction.HasValue,
                    p => p.IsDistributeTransaction == input.IsDistributeTransaction)
                .WhereIf(input.MultipleServiceKey.HasValue, p => p.MultipleServiceKey == input.MultipleServiceKey)
                .WhereIf(input.IsSystem.HasValue, p => p.IsSystem == input.IsSystem)
                .OrderBy(p => p.HostName)
                .ThenBy(p => p.ServiceId)
                .ToList();
            return serviceEntryOutputs.ToPagedList(input.PageIndex, input.PageSize);
        }

        private List<GetServiceEntryOutput> GetAllServiceEntryFromCache()
        {
            var serviceEntryOutputs = new List<GetServiceEntryOutput>();
            var servers = _serverManager.Servers;
            foreach (var server in servers)
            {
                foreach (var service in server.Services)
                {
                    var serviceEntries = service.ServiceEntries.Select(p =>
                        new GetServiceEntryOutput()
                        {
                            ServiceName = service.ServiceName,
                            ServiceId = service.Id,
                            ServiceEntryId = p.Id,
                            HostName = server.HostName,
                            IsEnable = server.Endpoints.Any(e =>
                                e.Enabled && e.ServiceProtocol == service.ServiceProtocol),
                            ServerInstanceCount = server.Endpoints.Count(e =>
                                e.ServiceProtocol == service.ServiceProtocol && e.Enabled),
                            ServiceProtocol = p.ServiceProtocol,
                            MultipleServiceKey = service.MultiServiceKeys(),
                            Author = p.GetAuthor(),
                            ProhibitExtranet = p.ProhibitExtranet,
                            IsAllowAnonymous = p.IsAllowAnonymous,
                            WebApi = p.WebApi,
                            HttpMethod = p.HttpMethod,
                            Method = p.Method,
                            IsSystem = service.IsSilkyService(),
                            IsDistributeTransaction = p.IsDistributeTransaction,
                            ServiceKeys = service.GetServiceKeys()?.Select(sk => new ServiceKeyOutput()
                            {
                                Name = sk.Key,
                                Weight = sk.Value
                            }).ToArray(),
                        });
                    serviceEntryOutputs.AddRange(serviceEntries);
                }
            }

            return serviceEntryOutputs;
        }

        public GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceEntryId)
        {
            var serviceEntryOutput =
                GetAllServiceEntryFromCache().FirstOrDefault(p => p.ServiceEntryId == serviceEntryId);
            if (serviceEntryOutput == null)
            {
                throw new BusinessException($"There is no service entry with id {serviceEntryId}");
            }

            var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(serviceEntryId);

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryOutput.ServiceEntryId);
            var serviceEntryDetailOutput = new GetServiceEntryDetailOutput()
            {
                HostName = serviceEntryOutput.HostName,
                ServiceEntryId = serviceEntryOutput.ServiceEntryId,
                ServiceId = serviceEntryOutput.ServiceId,
                ServiceName = serviceEntryOutput.ServiceName,
                ServiceProtocol = serviceEntryOutput.ServiceProtocol,
                Author = serviceEntryOutput.Author,
                WebApi = serviceEntryOutput.WebApi,
                HttpMethod = serviceEntryOutput.HttpMethod,
                ProhibitExtranet = serviceEntryOutput.ProhibitExtranet,
                Method = serviceEntryOutput.Method,
                MultipleServiceKey = serviceEntryOutput.MultipleServiceKey,
                IsEnable = serviceEntryOutput.IsEnable,
                ServerInstanceCount = serviceEntryOutput.ServerInstanceCount,
                Governance = serviceEntryDescriptor.GovernanceOptions,
                CacheTemplates = serviceEntry?.CustomAttributes.OfType<ICachingInterceptProvider>().Select(
                    p =>
                        new ServiceEntryCacheTemplateOutput()
                        {
                            KeyTemplete = p.KeyTemplete,
                            OnlyCurrentUserData = p.OnlyCurrentUserData,
                            CachingMethod = p.CachingMethod
                        }).ToArray(),
                ServiceKeys = serviceEntryOutput.ServiceKeys,
                IsDistributeTransaction = serviceEntryOutput.IsDistributeTransaction,
                Fallback = serviceEntry?.CustomAttributes.OfType<FallbackAttribute>().Select(p =>
                    new FallbackOutput()
                    {
                        TypeName = p.Type.FullName,
                        MethodName = p.MethodName ?? serviceEntry?.MethodInfo.Name,
                    }).FirstOrDefault()
            };

            return serviceEntryDetailOutput;
        }

        public PagedList<GetServiceEntryInstanceOutput> GetServiceEntryInstances(string serviceEntryId,
            int pageIndex = 1,
            int pageSize = 10)
        {
            var serviceEntryDescriptor = _serverManager.ServerDescriptors
                .SelectMany(p => p.Services.SelectMany(p => p.ServiceEntries))
                .FirstOrDefault(p => p.Id == serviceEntryId);
            if (serviceEntryDescriptor == null)
            {
                throw new BusinessException($"There is no service entry with id {serviceEntryId}");
            }

            var serverInstances = _serverManager.Servers.Where(p => p
                    .Services.Any(q => q.ServiceEntries.Any(e => e.Id == serviceEntryId)))
                .SelectMany(p => p.Endpoints).Where(p => p.ServiceProtocol == serviceEntryDescriptor.ServiceProtocol);


            var serviceEntryInstances = serverInstances
                .Where(p => p.ServiceProtocol == ServiceProtocol.Tcp)
                .Select(p =>
                    new GetServiceEntryInstanceOutput()
                    {
                        ServiceEntryId = serviceEntryId,
                        Address = p.Descriptor.GetHostAddress(),
                        Enabled = p.Enabled,
                        IsHealth = _serverHealthCheck.IsHealth(p).GetAwaiter().GetResult(),
                        ServiceProtocol = p.ServiceProtocol
                    });

            return serviceEntryInstances.ToPagedList(pageIndex, pageSize);
        }

        public async Task<ServerInstanceDetailInfo> GetInstanceDetail(string address)
        {
            if (!Regex.IsMatch(address, ipEndpointRegex))
            {
                throw new BusinessException($"{address} incorrect rpc address format");
            }

            var result = await _appointAddressInvoker.Invoke<ServerInstanceDetailInfo>(address,
                getInstanceSupervisorServiceEntryId, Array.Empty<object>());
            if (result?.Address != address)
            {
                throw new SilkyException("The rpc address of the routing instance is wrong");
            }

            return result;
        }

        public async Task<PagedList<ServerHandleInfo>> GetServiceEntryHandleInfos(string address,
            GetServerHandlePagedRequestDto input)
        {
            if (!Regex.IsMatch(address, ipEndpointRegex))
            {
                throw new BusinessException($"{address} incorrect rpcAddress format");
            }

            var result = await _appointAddressInvoker.Invoke<IReadOnlyCollection<ServerHandleInfo>>(address,
                getGetServiceEntrySupervisorServiceHandleServiceEntryId, Array.Empty<object>());

            return result
                .WhereIf(!input.ServiceEntryId.IsNullOrEmpty(), p => p.ServiceEntryId.Equals(input.ServiceEntryId))
                .WhereIf(!input.SearchKey.IsNullOrEmpty(), p =>
                    p.ServiceEntryId.Contains(input.SearchKey, StringComparison.OrdinalIgnoreCase)
                    || p.Address.Contains(input.SearchKey))
                .ToPagedList(input.PageIndex, input.PageSize);
        }

        private bool IsLocalAddress(string address)
        {
            var localAddress = RpcEndpointHelper.GetLocalTcpEndpoint().GetAddress();
            return localAddress.Equals(address);
        }


        public async Task<PagedList<ClientInvokeInfo>> GetServiceEntryInvokeInfos(string address,
            GetClientInvokePagedRequestDto input)
        {
            if (!Regex.IsMatch(address, ipEndpointRegex))
            {
                throw new BusinessException($"{address} incorrect rpc address format");
            }

            var serviceEntry =
                _serviceEntryLocator.GetServiceEntryById(getGetServiceEntrySupervisorServiceInvokeServiceEntryId);
            if (serviceEntry == null)
            {
                throw new BusinessException(
                    $"Not find serviceEntry by {getGetServiceEntrySupervisorServiceInvokeServiceEntryId}");
            }

            var result = await _appointAddressInvoker.Invoke<IReadOnlyCollection<ClientInvokeInfo>>(address,
                getGetServiceEntrySupervisorServiceInvokeServiceEntryId, Array.Empty<object>());

            return result
                .WhereIf(!input.ServiceEntryId.IsNullOrEmpty(), p => p.ServiceEntryId.Equals(input.ServiceEntryId))
                .WhereIf(!input.SearchKey.IsNullOrEmpty(), p =>
                    p.ServiceEntryId.Contains(input.SearchKey, StringComparison.OrdinalIgnoreCase)
                    || p.Address.Contains(input.SearchKey))
                .ToPagedList(input.PageIndex, input.PageSize);
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
                    RegistryCenterType = EngineContext.Current.Configuration.GetValue<string>("registrycenter:type")
                }).ToArray();
        }

        public IReadOnlyCollection<GetProfileOutput> GetProfiles()
        {
            var getProfileOutputs = new List<GetProfileOutput>();
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "Microservice",
                Title = "微服务应用",
                Count = _serverManager.ServerDescriptors.Count(p => p.Endpoints.Any(e => e.IsInstanceEndpoint()))
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceInstance",
                Title = "微服务应用实例",
                Count = _serverManager.ServerDescriptors.SelectMany(p => p.Endpoints).Count(p => p.IsInstanceEndpoint())
            });

            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "GatewayInstance",
                Title = "网关实例",
                Count = _serverManager.GetSelfServer().Endpoints.Count(e => e.ServiceProtocol.IsHttp())
            });

            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketService",
                Title = "支持WebSocket的微服务",
                Count = _serverManager.ServerDescriptors
                    .Count(p => p.Endpoints.Any(e => e.ServiceProtocol == ServiceProtocol.Ws))
            });

            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketServiceInstance",
                Title = "支持WebSocket的实例",
                Count = _serverManager.ServerDescriptors.SelectMany(p => p.Endpoints)
                    .Count(p => p.ServiceProtocol == ServiceProtocol.Ws)
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "Services",
                Title = "应用服务",
                Count = _serverManager.ServerDescriptors.SelectMany(p => p.Services)
                    .Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketService",
                Title = "WebSocket服务",
                Count = _serverManager.ServerDescriptors.SelectMany(p => p.Services)
                    .Where(p => p.ServiceProtocol == ServiceProtocol.Ws)
                    .Distinct().Count()
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceEntry",
                Title = "服务条目",
                Count = _serverManager.ServerDescriptors.SelectMany(p => p.Services.SelectMany(p => p.ServiceEntries))
                    .Distinct().Count()
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
                    if (externalRoute.Children.All(p => p.Path != externalRouteChild.Path))
                    {
                        externalRoute.Children.Add(externalRouteChild);
                    }
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