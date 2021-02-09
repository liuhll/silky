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
        protected ServiceRouteManagerBase(ServiceRouteCache serviceRouteCache,
            IServiceEntryManager serviceEntryManager, IOptions<RegistryCenterOptions> registryCenterOptions)
        {
            _serviceRouteCache = serviceRouteCache;
            _serviceEntryManager = serviceEntryManager;
            _registryCenterOptions = registryCenterOptions.Value;
            Check.NotNullOrEmpty(_registryCenterOptions.RoutePath,nameof(_registryCenterOptions.RoutePath));
            Check.NotNullOrEmpty(_registryCenterOptions.CommandPath,nameof(_registryCenterOptions.CommandPath));
            
        }
        
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
                    serviceRouteDescriptor.AddressDescriptors = serviceRouteDescriptor.AddressDescriptors.Concat(centreServiceRoute.AddressDescriptors).Distinct().OrderBy(p=> p.ToString());
                }
                await RegisterRouteAsync(serviceRouteDescriptor);
            }

        }

        public abstract Task EnterRoutes(ServiceProtocol serviceProtocol);

        protected abstract Task CreateSubDirectory(ServiceProtocol serviceProtocol);

        protected abstract Task RegisterRouteAsync(ServiceRouteDescriptor serviceRouteDescriptor);

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
                        await RegisterRouteAsync(removeRoute);
                    }
                }
            }
        }

    }
}