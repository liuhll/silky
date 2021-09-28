using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Rpc;
using Silky.Http.Dashboard.AppService.Dtos;
using Silky.Http.Dashboard.Configuration;
using Silky.Rpc.AppServices.Dtos;
using Silky.Rpc.CachingInterceptor.Providers;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.RegistryCenters;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Http.Dashboard.AppService
{
    public class SilkyAppService : ISilkyAppService
    {
        private readonly IServerManager _serverManager;
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IRemoteExecutor _remoteExecutor;
        private readonly ILocalExecutor _localExecutor;
        private readonly IRegisterCenterHealthProvider _registerCenterHealthProvider;


        private const string ipEndpointRegex =
            @"([0-9]|[1-9]\d{1,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])";

        private const string getInstanceSupervisorServiceEntryId =
            "Silky.Rpc.AppServices.IRpcAppService.GetInstanceDetail_Get";

        private const string getGetServiceEntrySupervisorServiceHandle =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntryHandleInfos_Get";

        private const string getGetServiceEntrySupervisorServiceInvoke =
            "Silky.Rpc.AppServices.IRpcAppService.GetServiceEntryInvokeInfos_Get";

        public SilkyAppService(
            IServerManager serverManager,
            IServiceEntryManager serviceEntryManager,
            IServiceEntryLocator serviceEntryLocator,
            IRemoteExecutor remoteExecutor,
            ILocalExecutor localExecutor,
            IRegisterCenterHealthProvider registerCenterHealthProvider)
        {
            _serverManager = serverManager;
            _serviceEntryManager = serviceEntryManager;

            _remoteExecutor = remoteExecutor;
            _registerCenterHealthProvider = registerCenterHealthProvider;
            _localExecutor = localExecutor;
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
                .Select(p => new GetHostOutput()
                {
                    AppServiceCount = p.Services.Length,
                    InstanceCount = p.Endpoints.Count(p => p.IsInstanceEndpoint()),
                    Host = p.HostName,
                    HasWsService = p.Endpoints.Any(p => p.ServiceProtocol == ServiceProtocol.Ws),
                    LocalServiceEntriesCount = p.Services.SelectMany(p => p.ServiceEntries).Count()
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

        public IReadOnlyCollection<ServiceDescriptor> GetServices(string hostName)
        {
            return GetHostDetail(hostName).Services;
        }

        public PagedList<GetHostInstanceOutput> GetHostInstances(string hostName,
            GetHostInstanceInput input)
        {
            var server = _serverManager.GetServer(hostName);
            if (server == null)
            {
                throw new BusinessException($"There is no server for {hostName}");
            }

            var hostInstances = server.Endpoints
                .WhereIf(input.ServiceProtocol.HasValue, p => p.ServiceProtocol == input.ServiceProtocol)
                .WhereIf(!input.ServiceProtocol.HasValue, p => p.Descriptor.IsInstanceEndpoint())
                .Select(p => new GetHostInstanceOutput()
                {
                    HostName = server.HostName,
                    IsHealth = SocketCheck.TestConnection(p.Host, p.Port),
                    Address = p.Host,
                    IsEnable = p.Enabled,
                    ServiceProtocol = p.ServiceProtocol,
                });

            return hostInstances.ToPagedList(input.PageIndex, input.PageSize);
        }

        public GetGatewayOutput GetGateway()
        {
            var gateway = _serverManager.GetServer(EngineContext.Current.HostName);
            return new GetGatewayOutput()
            {
                HostName = gateway.HostName,
                InstanceCount = gateway.Endpoints.Count(p => p.ServiceProtocol.IsHttp())
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
                    Address = addressDescriptor.Host,
                    Port = addressDescriptor.Port,
                };
                gatewayInstances.Add(gatewayInstance);
            }

            return gatewayInstances.ToPagedList(input.PageIndex, input.PageSize);
        }

        public PagedList<GetServiceEntryOutput> GetServiceEntries(GetServiceEntryInput input)
        {
            var serviceEntryOutputs = GetAllServiceEntryFromCache();

            serviceEntryOutputs = serviceEntryOutputs
                .WhereIf(!input.HostName.IsNullOrEmpty(), p => input.HostName.Equals(p.HostName))
                .WhereIf(!input.ServiceId.IsNullOrEmpty(), p => input.ServiceId.Equals(p.ServiceId))
                .WhereIf(!input.ServiceEntryId.IsNullOrEmpty(), p => input.ServiceEntryId.Equals(p.ServiceEntryId))
                .WhereIf(!input.Name.IsNullOrEmpty(), p => p.ServiceEntryId.Contains(input.Name))
                .WhereIf(input.IsEnable.HasValue, p => p.IsEnable == input.IsEnable)
                .OrderBy(p => p.HostName)
                .ThenBy(p => p.ServiceId)
                .ToList();
            return serviceEntryOutputs.ToPagedList(input.PageIndex, input.PageSize);
        }

        private List<GetServiceEntryOutput> GetAllServiceEntryFromCache()
        {
            var serviceEntryOutputs = new List<GetServiceEntryOutput>();
            var servers = _serverManager.ServerDescriptors;
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
                            IsEnable = server.Endpoints.Any(),
                            MultipleServiceKey = service.MultiServiceKeys(),
                            Author = p.GetAuthor(),
                            ProhibitExtranet = p.ProhibitExtranet,
                            WebApi = p.WebApi,
                            HttpMethod = p.HttpMethod,
                            Method = p.Method,
                            ServiceKeys = service.GetServiceKeys()?.Select(p => new ServiceKeyOutput()
                            {
                                Name = p.Key,
                                Weight = p.Value
                            }).ToArray(),
                        });
                    serviceEntryOutputs.AddRange(serviceEntries);
                }
            }

            return serviceEntryOutputs;
        }

        public GetServiceEntryDetailOutput GetServiceEntryDetail(string serviceEntryId)
        {
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryId);
            if (serviceEntry == null)
            {
                throw new BusinessException($"There is no service entry with id {serviceEntryId}");
            }

            var serviceEntryOutput = GetAllServiceEntryFromCache().First(p => p.ServiceEntryId == serviceEntryId);
            var serviceEntryDetailOutput = new GetServiceEntryDetailOutput()
            {
                HostName = serviceEntryOutput.HostName,
                ServiceEntryId = serviceEntry.Id,
                ServiceId = serviceEntry.ServiceEntryDescriptor.ServiceId,
                ServiceName = serviceEntry.ServiceEntryDescriptor.ServiceName,
                Author = serviceEntry.ServiceEntryDescriptor.GetAuthor(),
                WebApi = serviceEntry.GovernanceOptions.ProhibitExtranet ? "" : serviceEntry.Router.RoutePath,
                HttpMethod = serviceEntry.GovernanceOptions.ProhibitExtranet ? null : serviceEntry.Router.HttpMethod,
                ProhibitExtranet = serviceEntry.GovernanceOptions.ProhibitExtranet,
                Method = serviceEntry.MethodInfo.Name,
                MultipleServiceKey = serviceEntryOutput.MultipleServiceKey,
                IsEnable = serviceEntryOutput.IsEnable,
                ServerInstanceCount = serviceEntryOutput.ServerInstanceCount,
                GovernanceOptions = serviceEntry.GovernanceOptions,
                CacheTemplates = serviceEntry.CustomAttributes.OfType<ICachingInterceptProvider>().Select(p =>
                    new ServiceEntryCacheTemplateOutput()
                    {
                        KeyTemplete = p.KeyTemplete,
                        OnlyCurrentUserData = p.OnlyCurrentUserData,
                        CachingMethod = p.CachingMethod
                    }).ToArray(),
                ServiceKeys = serviceEntryOutput.ServiceKeys,
                IsDistributeTransaction = serviceEntry.IsTransactionServiceEntry()
            };

            return serviceEntryDetailOutput;
        }

        public PagedList<GetServiceEntryInstanceOutput> GetServiceEntryRoutes(string serviceEntryId, int pageIndex = 1,
            int pageSize = 10)
        {
            var serviceEntry = _serviceEntryManager.GetAllEntries().FirstOrDefault(p => p.Id == serviceEntryId);
            if (serviceEntry == null)
            {
                throw new BusinessException($"There is no service entry with id {serviceEntryId}");
            }

            var serverInstances = _serverManager.ServerDescriptors.Where(p => p
                    .Services.Any(q => q.ServiceEntries.Any(e => e.Id == serviceEntryId)))
                .SelectMany(p => p.Endpoints);

            var serviceEntryInstances = serverInstances.Select(p => new GetServiceEntryInstanceOutput()
            {
                ServiceName = serviceEntry.ServiceEntryDescriptor.ServiceName,
                ServiceId = serviceEntry.ServiceId,
                ServiceEntryId = serviceEntry.Id,
                Address = p.GetHostAddress(),
                ServiceProtocol = p.ServiceProtocol
            });

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
                Count = _serverManager.ServerDescriptors.Count
            });
            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "ServiceInstance",
                Title = "微服务应用实例",
                Count = _serverManager.ServerDescriptors.SelectMany(p => p.Endpoints).Count(p => p.IsInstanceEndpoint())
            });

            getProfileOutputs.Add(new GetProfileOutput()
            {
                Code = "WebSocketServiceInstance",
                Title = "支持WebSocket的应用实例",
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