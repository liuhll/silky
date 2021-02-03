using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry;
using Lms.Rpc.Utils;

namespace Lms.Rpc.Routing
{
    public abstract class ServiceRouteManagerBase : IServiceRouteManager
    {
        protected readonly ServiceRouteCache _serviceRouteCache;
        protected readonly IServiceEntryManager _serviceEntryManager;
        protected ServiceRouteManagerBase(ServiceRouteCache serviceRouteCache,
            IServiceEntryManager serviceEntryManager)
        {
            _serviceRouteCache = serviceRouteCache;
            _serviceEntryManager = serviceEntryManager;
            EnterRoutes().GetAwaiter().GetResult();
        }

        protected abstract Task EnterRoutes();



        public virtual async Task RegisterRoutes(double processorTime, ServiceProtocol serviceProtocol)
        {
            var localServiceEntries = _serviceEntryManager.GetLocalEntries().Where(p=> p.ServiceDescriptor.ServiceProtocol == serviceProtocol);
            var serviceRouteDescriptors = localServiceEntries.Select(p => p.CreateLocalRouteDescriptor());
            var registrationCentreServiceRoutes = _serviceRouteCache.ServiceRouteDescriptors.Where(p =>
                serviceRouteDescriptors.Any(q => q.ServiceDescriptor.Equals(p.ServiceDescriptor)));
            var centreServiceRoutes = registrationCentreServiceRoutes as ServiceRouteDescriptor[] ?? registrationCentreServiceRoutes.ToArray();
            if (centreServiceRoutes.Any())
            {
                await RemoveExceptRouteAsyncs(registrationCentreServiceRoutes);
                foreach (var registrationCentreServiceRoute in centreServiceRoutes)
                {
                    var serviceRouteDescriptor =
                        serviceRouteDescriptors.Single(p => p.ServiceDescriptor.Equals(registrationCentreServiceRoute.ServiceDescriptor));
                    serviceRouteDescriptor.AddressDescriptors = serviceRouteDescriptor.AddressDescriptors.Concat(registrationCentreServiceRoute.AddressDescriptors).Distinct();
                }
            }

            foreach (var serviceRouteDescriptor in serviceRouteDescriptors)
            {
                await SetRouteAsync(serviceRouteDescriptor);
            }

        }
        

        protected abstract Task SetRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor);

        protected virtual async Task RemoveExceptRouteAsyncs(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors)
        {
           
            var oldServiceDescriptorIds = _serviceRouteCache.ServiceRouteDescriptors.Select(i => i.ServiceDescriptor.Id).ToArray();
            var newServiceDescriptorIds = serviceRouteDescriptors.Select(i => i.ServiceDescriptor.Id).ToArray();
            var removeServiceDescriptorIds = oldServiceDescriptorIds.Except(newServiceDescriptorIds).ToArray();
            foreach (var removeServiceDescriptorId in removeServiceDescriptorIds)
            {
               
                var removeRoute = _serviceRouteCache.ServiceRouteDescriptors.FirstOrDefault(p => p.ServiceDescriptor.Id == removeServiceDescriptorId);
                if (removeRoute != null && removeRoute.AddressDescriptors.Any())
                {
                    var hostAddr = NetUtil.GetHostAddress(removeRoute.ServiceDescriptor.ServiceProtocol);
                    if (removeRoute.AddressDescriptors.Any(p=> p.Equals(hostAddr)))
                    {
                        removeRoute.AddressDescriptors = removeRoute.AddressDescriptors.Where(p => !p.Equals(hostAddr)).ToList();
                        await SetRouteAsync(removeRoute);
                    }
                }
            }
        }

    }
}