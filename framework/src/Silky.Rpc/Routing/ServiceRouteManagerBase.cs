using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Microsoft.Extensions.Options;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Runtime.Server.Descriptor;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Routing
{
    public abstract class ServiceRouteManagerBase : IServiceRouteManager
    {
        protected readonly ServiceRouteCache _serviceRouteCache;
        protected readonly IServiceEntryManager _serviceEntryManager;
        protected readonly RegistryCenterOptions _registryCenterOptions;
        protected readonly RpcOptions _rpcOptions;

        protected ServiceRouteManagerBase(ServiceRouteCache serviceRouteCache,
            IServiceEntryManager serviceEntryManager,
            IOptions<RegistryCenterOptions> registryCenterOptions,
            IOptions<RpcOptions> rpcOptions)
        {
            _serviceRouteCache = serviceRouteCache;
            _serviceEntryManager = serviceEntryManager;
            _registryCenterOptions = registryCenterOptions.Value;
            _rpcOptions = rpcOptions.Value;
            Check.NotNullOrEmpty(_registryCenterOptions.RoutePath, nameof(_registryCenterOptions.RoutePath));
            Check.NotNullOrEmpty(_rpcOptions.Token, nameof(_rpcOptions.Token));
            _serviceRouteCache.OnRemoveServiceRoutes += async descriptors =>
            {
                if (_rpcOptions.RemoveUnhealthServer)
                {
                    foreach (var descriptor in descriptors)
                    {
                        await RegisterRouteAsync(descriptor);
                    }
                }
            };
        }

        public abstract Task CreateSubscribeDataChanges();

        public abstract Task CreateWsSubscribeDataChanges(string[] wsPaths);

        public abstract Task EnterRoutes();

        public virtual async Task RegisterRpcRoutes(double processorTime, ServiceProtocol serviceProtocol)
        {
            var hostAddr = NetUtil.GetRpcAddressModel();
            var localServiceEntries = _serviceEntryManager.GetLocalEntries()
                .Where(p => p.ServiceDescriptor.ServiceProtocol == serviceProtocol);
            var serviceRouteDescriptors = localServiceEntries.Select(p => p.CreateLocalRouteDescriptor());
            await RegisterRoutes(serviceRouteDescriptors, hostAddr.Descriptor);
        }

        public virtual async Task RegisterWsRoutes(double processorTime, Type[] wsAppServiceTypes, int wsPort)
        {
            var hostAddr = NetUtil.GetAddressModel(wsPort, ServiceProtocol.Ws);
            var serviceRouteDescriptors = wsAppServiceTypes.Select(p => new ServiceRouteDescriptor()
            {
                ServiceDescriptor = new ServiceDescriptor()
                {
                    Id = WebSocketResolverHelper.Generator(WebSocketResolverHelper.ParseWsPath(p)),
                    ServiceProtocol = ServiceProtocol.Ws,
                },
                AddressDescriptors = new[]
                {
                    hostAddr.Descriptor
                },
            });

            await RegisterRoutes(serviceRouteDescriptors, hostAddr.Descriptor);
        }

        protected virtual async Task RegisterRoutes(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors,
            AddressDescriptor addressDescriptor)
        {
            await CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();
            await RemoveExceptRouteAsync(serviceRouteDescriptors, addressDescriptor);
            foreach (var serviceRouteDescriptor in serviceRouteDescriptors)
            {
                await RegisterRouteAsync(serviceRouteDescriptor);
            }
        }

        protected abstract Task CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();

        protected abstract Task RegisterRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor);

        protected virtual async Task RemoveExceptRouteAsync(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors,
            AddressDescriptor addressDescriptor)
        {
            var oldServiceDescriptorIds =
                _serviceRouteCache.ServiceRouteDescriptors.Select(i => i.ServiceDescriptor.Id).ToArray();

            var newServiceDescriptorIds = serviceRouteDescriptors.Select(i => i.ServiceDescriptor.Id).ToArray();

            var checkServiceDescriptorIds = oldServiceDescriptorIds.Except(newServiceDescriptorIds).ToArray();

            foreach (var checkServiceDescriptorId in checkServiceDescriptorIds)
            {
                var removeRoute =
                    _serviceRouteCache.ServiceRouteDescriptors.FirstOrDefault(p =>
                        p.ServiceDescriptor.Id == checkServiceDescriptorId);

                if (removeRoute != null && removeRoute.AddressDescriptors.Any())
                {
                    if (removeRoute.AddressDescriptors.Any(p => p.Equals(addressDescriptor)))
                    {
                        removeRoute.AddressDescriptors =
                            removeRoute.AddressDescriptors.Where(p => !p.Equals(addressDescriptor)).ToList();
                        await RegisterRouteAsync(removeRoute);
                    }
                }
            }
        }
    }
}