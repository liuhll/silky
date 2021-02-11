using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Rpc.Configuration;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Utils;
using Microsoft.Extensions.Options;

namespace Lms.Rpc.Routing
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
            Check.NotNullOrEmpty(_registryCenterOptions.CommandPath, nameof(_registryCenterOptions.CommandPath));
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

        public abstract Task EnterRoutes(ServiceProtocol serviceProtocol);

        public virtual async Task RegisterRoutes(double processorTime, ServiceProtocol serviceProtocol)
        {
            var localServiceEntries = _serviceEntryManager.GetLocalEntries()
                .Where(p => p.ServiceDescriptor.ServiceProtocol == serviceProtocol);
            var serviceRouteDescriptors = localServiceEntries.Select(p => p.CreateLocalRouteDescriptor());
            var registrationCentreServiceRoutes = _serviceRouteCache.ServiceRouteDescriptors.Where(p =>
                serviceRouteDescriptors.Any(q => q.ServiceDescriptor.Equals(p.ServiceDescriptor)));
            var centreServiceRoutes = registrationCentreServiceRoutes as ServiceRouteDescriptor[] ??
                                      registrationCentreServiceRoutes.ToArray();
            if (centreServiceRoutes.Any())
            {
                await RemoveExceptRouteAsyncs(registrationCentreServiceRoutes);
            }
            else
            {
                await CreateSubDirectory(serviceProtocol);
            }

            foreach (var serviceRouteDescriptor in serviceRouteDescriptors)
            {
                var centreServiceRoute = registrationCentreServiceRoutes.SingleOrDefault(p =>
                    p.ServiceDescriptor.Equals(serviceRouteDescriptor.ServiceDescriptor));
                if (centreServiceRoute != null)
                {
                    serviceRouteDescriptor.AddressDescriptors = serviceRouteDescriptor.AddressDescriptors
                        .Concat(centreServiceRoute.AddressDescriptors).Distinct().OrderBy(p => p.ToString());
                }

                await RegisterRouteAsync(serviceRouteDescriptor);
            }
        }

        protected abstract Task CreateSubDirectory(ServiceProtocol serviceProtocol);

        protected abstract Task RegisterRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor);

        protected virtual async Task RemoveExceptRouteAsyncs(
            IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors)
        {
            var oldServiceDescriptorIds =
                _serviceRouteCache.ServiceRouteDescriptors.Select(i => i.ServiceDescriptor.Id).ToArray();
            var newServiceDescriptorIds = serviceRouteDescriptors.Select(i => i.ServiceDescriptor.Id).ToArray();
            var removeServiceDescriptorIds = oldServiceDescriptorIds.Except(newServiceDescriptorIds).ToArray();
            foreach (var removeServiceDescriptorId in removeServiceDescriptorIds)
            {
                var removeRoute =
                    _serviceRouteCache.ServiceRouteDescriptors.FirstOrDefault(p =>
                        p.ServiceDescriptor.Id == removeServiceDescriptorId);
                if (removeRoute != null && removeRoute.AddressDescriptors.Any())
                {
                    var hostAddr = NetUtil.GetHostAddress(removeRoute.ServiceDescriptor.ServiceProtocol);
                    if (removeRoute.AddressDescriptors.Any(p => p.Equals(hostAddr)))
                    {
                        removeRoute.AddressDescriptors =
                            removeRoute.AddressDescriptors.Where(p => !p.Equals(hostAddr)).ToList();
                        await RegisterRouteAsync(removeRoute);
                    }
                }
            }
        }
    }
}