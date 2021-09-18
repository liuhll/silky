using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;
using Microsoft.Extensions.Options;
using Silky.Rpc.Address;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing
{
    public abstract class ServiceRouteManagerBase : IServiceRouteManager
    {
        protected readonly ServiceRouteCache _serviceRouteCache;
        protected readonly IServiceManager _serviceManager;
        protected RegistryCenterOptions _registryCenterOptions;
        protected RpcOptions _rpcOptions;
        public ILogger<ServiceRouteManagerBase> Logger { get; set; }

        protected ServiceRouteManagerBase(ServiceRouteCache serviceRouteCache,
            IServiceManager serviceManager,
            IOptionsMonitor<RegistryCenterOptions> registryCenterOptions,
            IOptionsMonitor<RpcOptions> rpcOptions)
        {
            _serviceRouteCache = serviceRouteCache;
            _serviceManager = serviceManager;
            _registryCenterOptions = registryCenterOptions.CurrentValue;

            _rpcOptions = rpcOptions.CurrentValue;
            rpcOptions.OnChange((options, s) => _rpcOptions = options);

            Check.NotNullOrEmpty(_registryCenterOptions.RoutePath, nameof(_registryCenterOptions.RoutePath));
            Check.NotNullOrEmpty(_rpcOptions.Token, nameof(_rpcOptions.Token));
            Logger = NullLogger<ServiceRouteManagerBase>.Instance;
            _serviceRouteCache.OnRemoveServiceRoutes += async (descriptors, addressModel) =>
            {
                foreach (var descriptor in descriptors)
                {
                    await RemoveUnHealthServiceRoute(descriptor.Service.Id, addressModel);
                }
            };
            _serviceRouteCache.OnRemoveServiceRoute += async (serviceId, addressModel) =>
            {
                await RemoveServiceRoute(serviceId, addressModel);
            };
        }

        public virtual async Task RegisterRpcRoutes(AddressDescriptor addressDescriptor,
            ServiceProtocol serviceProtocol)
        {
            await CreateSubscribeServiceRouteDataChanges(serviceProtocol);
            var localServices = _serviceManager.GetLocalService()
                .Where(p => p.ServiceProtocol == serviceProtocol);
            Logger.LogDebug(
                $"Preparing to register routing data for the server [{addressDescriptor.GetHostAddress()}]," +
                $"including the following services: {Environment.NewLine}" +
                $"{string.Join($",", localServices.Select(p => p.ServiceDescriptor.ServiceName))}");
            var serviceRouteDescriptors = localServices.Select(p => p.CreateLocalRouteDescriptor(addressDescriptor));
            await RegisterRoutes(serviceRouteDescriptors, addressDescriptor);
            Logger.LogDebug(
                $"The the server [{addressDescriptor.GetHostAddress()}] routing data is successfully registered");
        }

        protected virtual async Task RegisterRoutes(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors,
            AddressDescriptor addressDescriptor)
        {
            await CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();
            await RemoveServiceCenterExceptRoute(addressDescriptor);
            foreach (var serviceRouteDescriptor in serviceRouteDescriptors)
            {
                await RegisterRouteServiceCenter(serviceRouteDescriptor);
            }
        }

        protected abstract Task RemoveUnHealthServiceRoute(string serviceId, IRpcAddress rpcAddress);

        protected abstract Task CreateSubscribeServiceRouteDataChanges(ServiceProtocol serviceProtocol);

        public virtual async Task EnterRoutes()
        {
            await EnterRoutesFromServiceCenter();
        }

        protected abstract Task EnterRoutesFromServiceCenter();
        protected abstract Task CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();

        protected abstract Task RegisterRouteServiceCenter(ServiceRouteDescriptor serviceRouteDescriptor);
        protected abstract Task RemoveServiceCenterExceptRoute(AddressDescriptor addressDescriptor);

        private async Task RemoveServiceRoute(string serviceId, IRpcAddress selectedRpcAddress)
        {
            await RemoveUnHealthServiceRoute(serviceId, selectedRpcAddress);
        }
    }
}