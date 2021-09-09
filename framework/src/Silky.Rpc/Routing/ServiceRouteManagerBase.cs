using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Microsoft.Extensions.Options;
using Silky.Rpc.Address;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Routing
{
    public abstract class ServiceRouteManagerBase : IServiceRouteManager
    {
        protected readonly ServiceRouteCache _serviceRouteCache;
        protected readonly IServiceManager _serviceManager;
        protected RegistryCenterOptions _registryCenterOptions;
        protected RpcOptions _rpcOptions;

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

        protected abstract Task RemoveUnHealthServiceRoute(string serviceId, IAddressModel addressModel);


        public abstract Task CreateSubscribeServiceRouteDataChanges();
        
        
        public abstract Task EnterRoutes();

        public async Task RemoveServiceRoute(string serviceId, IAddressModel selectedAddress)
        {
            await RemoveUnHealthServiceRoute(serviceId, selectedAddress);
        }

        public virtual async Task RegisterRpcRoutes(AddressDescriptor addressDescriptor, ServiceProtocol serviceProtocol)
        {
          
            var localServices = _serviceManager.GetLocalService()
                .Where(p => p.ServiceProtocol == serviceProtocol);
            var serviceRouteDescriptors = localServices.Select(p => p.CreateLocalRouteDescriptor(addressDescriptor));
            await RegisterRoutes(serviceRouteDescriptors, addressDescriptor);
        }
        
        protected virtual async Task RegisterRoutes(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors,
            AddressDescriptor addressDescriptor)
        {
            await CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();
            await RemoveExceptRouteAsync(addressDescriptor);
            foreach (var serviceRouteDescriptor in serviceRouteDescriptors)
            {
                await RegisterRouteAsync(serviceRouteDescriptor);
            }
        }

        protected abstract Task CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();

        protected abstract Task RegisterRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor);

        protected abstract Task RemoveExceptRouteAsync(AddressDescriptor addressDescriptor);
    }
}