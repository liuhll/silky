using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Lms.Rpc.Address;
using Lms.Rpc.Routing.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry;
using Lms.Rpc.Utils;

namespace Lms.Rpc.Routing
{
    public abstract class ServiceRouteManagerBase : IServiceRouteManager
    {
        protected readonly ServiceRouteCache _serviceRouteCache;

        protected ServiceRouteManagerBase(ServiceRouteCache serviceRouteCache)
        {
            _serviceRouteCache = serviceRouteCache;
            EnterRoutes().GetAwaiter().GetResult();
        }

        protected abstract Task EnterRoutes();



        public virtual async Task RegisterRoutes(IReadOnlyList<ServiceEntry> localServiceEntries, AddressType address)
        {
            if (!localServiceEntries.All(p=>p.IsLocal))
            {
                throw new LmsException("注册服务路由时存在远程服务条目,只能注册本地服务条目的服务路由");
            }

            var serviceRouteDescriptors = localServiceEntries.Select(p => p.CreateLocalRouteDescriptor(address));
            var registrationCentreServiceRoutes = _serviceRouteCache.ServiceRouteDescriptors.Where(p =>
                serviceRouteDescriptors.Any(q => q.ServiceDescriptor.Equals(p.ServiceDescriptor)));
            var centreServiceRoutes = registrationCentreServiceRoutes as ServiceRouteDescriptor[] ?? registrationCentreServiceRoutes.ToArray();
            if (centreServiceRoutes.Any())
            {
                await RemoveExceptRouteAsyncs(registrationCentreServiceRoutes, NetUtil.GetHostAddress(address));
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

        protected virtual async Task RemoveExceptRouteAsyncs(IEnumerable<ServiceRouteDescriptor> serviceRouteDescriptors,
            IAddressModel hostAddress)
        {
            
        }

    }
}