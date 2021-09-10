using System.Threading.Tasks;
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
        protected readonly IRouteDescriptorProvider _routeDescriptorProvider;
        protected RegistryCenterOptions _registryCenterOptions;
        protected RpcOptions _rpcOptions;

        protected ServiceRouteManagerBase(ServiceRouteCache serviceRouteCache,
            IRouteDescriptorProvider routeDescriptorProvider,
            IOptionsMonitor<RegistryCenterOptions> registryCenterOptions,
            IOptionsMonitor<RpcOptions> rpcOptions)
        {
            _serviceRouteCache = serviceRouteCache;
            _routeDescriptorProvider = routeDescriptorProvider;
            _registryCenterOptions = registryCenterOptions.CurrentValue;

            _rpcOptions = rpcOptions.CurrentValue;
            rpcOptions.OnChange((options, s) => _rpcOptions = options);

            Check.NotNullOrEmpty(_registryCenterOptions.RoutePath, nameof(_registryCenterOptions.RoutePath));
            Check.NotNullOrEmpty(_rpcOptions.Token, nameof(_rpcOptions.Token));
            _serviceRouteCache.OnRemoveServiceRoutes += async (descriptors, addressModel) =>
            {
                foreach (var descriptor in descriptors)
                {
                    await RemoveUnHealthServiceRoute(descriptor.HostName, addressModel);
                }
            };
            _serviceRouteCache.OnRemoveServiceRoute += async (hostName, addressModel) =>
            {
                await RemoveServiceRoute(hostName, addressModel);
            };
        }

        protected abstract Task RemoveUnHealthServiceRoute(string hostName, IAddressModel addressModel);


        public abstract Task CreateSubscribeServiceRouteDataChanges();


        public abstract Task EnterRoutes();

        public async Task RemoveServiceRoute(string hostName, IAddressModel selectedAddress)
        {
            await RemoveUnHealthServiceRoute(hostName, selectedAddress);
        }

        public virtual async Task RegisterRpcRoutes(AddressDescriptor addressDescriptor,
            ServiceProtocol serviceProtocol)
        {
            var serviceRouteDescriptor = _routeDescriptorProvider.Create(addressDescriptor, serviceProtocol);
            await RegisterRoutes(serviceRouteDescriptor, addressDescriptor);
        }

        protected virtual async Task RegisterRoutes(RouteDescriptor serviceRouteDescriptor,
            AddressDescriptor addressDescriptor)

        {
            await CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();
            await RemoveExceptRouteAsync(addressDescriptor);
            await RegisterRouteWithServiceRegistry(serviceRouteDescriptor);
        }

        protected abstract Task CreateSubDirectoryIfNotExistAndSubscribeChildrenChange();

        protected abstract Task RegisterRouteWithServiceRegistry(RouteDescriptor routeDescriptor);

        protected abstract Task RemoveExceptRouteAsync(AddressDescriptor addressDescriptor);
    }
}